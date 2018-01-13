const gulp = require('gulp');
const webpackStream = require('webpack-stream');
const livereload = require('gulp-livereload');
const path = require('path');
const webpack = require('webpack');
const errorHandler = require('./helpers/errorHandler');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

const uiFolder = 'UI';
const root = path.join(__dirname, '..', 'src');
const isProduction = process.argv.indexOf('--production') > -1;

console.log('ROOT:', root);
console.log('isProduction:', isProduction);

const cssVarsFiles = [
  '../src/Styles/Variables/colors',
  '../src/Styles/Variables/dimensions',
  '../src/Styles/Variables/fonts',
  '../src/Styles/Variables/animations'
].map(require.resolve);

const extractCSSPlugin = new ExtractTextPlugin({
  filename: path.join('_output', uiFolder, 'Content', 'styles.css'),
  allChunks: true,
  disable: false,
  ignoreOrder: true
});

const plugins = [
  extractCSSPlugin,

  new webpack.optimize.CommonsChunkPlugin({
    name: 'vendor'
  }),

  new webpack.DefinePlugin({
    __DEV__: !isProduction,
    'process.env.NODE_ENV': isProduction ? JSON.stringify('production') : JSON.stringify('development')
  })
];

if (isProduction) {
  plugins.push(new UglifyJSPlugin({
    sourceMap: true,
    uglifyOptions: {
      mangle: false,
      output: {
        comments: false,
        beautify: true
      }
    }
  }));
}

const config = {
  devtool: '#source-map',

  stats: {
    children: false
  },

  watchOptions: {
    ignored: /node_modules/
  },

  entry: {
    preload: 'preload.js',
    vendor: 'vendor.js',
    index: 'index.js'
  },

  resolve: {
    modules: [
      root,
      path.join(root, 'Shims'),
      'node_modules'
    ],
    alias: {
      jquery: 'jquery/src/jquery'
    }
  },

  output: {
    filename: path.join('_output', uiFolder, '[name].js'),
    sourceMapFilename: '[file].map'
  },

  plugins,

  resolveLoader: {
    modules: [
      'node_modules',
      'frontend/gulp/webpack/'
    ]
  },

  module: {
    rules: [
      {
        test: /\.js?$/,
        exclude: /(node_modules|JsLibraries)/,
        loader: 'babel-loader',
        query: {
          plugins: ['transform-class-properties'],
          presets: ['es2015', 'decorators-legacy', 'react', 'stage-2'],
          env: {
            development: {
              plugins: ['transform-react-jsx-source']
            }
          }
        }
      },

      // CSS Modules
      {
        test: /\.css$/,
        exclude: /(node_modules|globals.css)/,
        use: extractCSSPlugin.extract({
          fallback: 'style-loader',
          use: [
            {
              loader: 'css-variables-loader',
              options: {
                cssVarsFiles
              }
            },
            {
              loader: 'css-loader',
              options: {
                modules: true,
                importLoaders: 1,
                localIdentName: '[name]-[local]-[hash:base64:5]',
                sourceMap: true
              }
            },
            {
              loader: 'postcss-loader',
              options: {
                config: {
                  ctx: {
                    cssVarsFiles
                  },
                  path: 'frontend/postcss.config.js'
                }
              }
            }
          ]
        })
      },

      // Global styles
      {
        test: /\.css$/,
        include: /(node_modules|globals.css)/,
        use: [
          'style-loader',
          {
            loader: 'css-loader'
          }
        ]
      },

      // Fonts
      {
        test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        use: [
          {
            loader: 'url-loader',
            options: {
              limit: 10240,
              mimetype: 'application/font-woff',
              emitFile: false,
              name: 'Content/Fonts/[name].[ext]'
            }
          }
        ]
      },

      {
        test: /\.(ttf|eot|eot?#iefix|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        use: [
          {
            loader: 'file-loader',
            options: {
              emitFile: false,
              name: 'Content/Fonts/[name].[ext]'
            }
          }
        ]
      }
    ]
  }
};

gulp.task('webpack', () => {
  return gulp.src('index.js')
    .pipe(webpackStream(config))
    .pipe(gulp.dest(''));
});

gulp.task('webpackWatch', () => {
  config.watch = true;
  return gulp.src('')
    .pipe(webpackStream(config))
    .on('error', errorHandler)
    .pipe(gulp.dest(''))
    .on('error', errorHandler)
    .pipe(livereload())
    .on('error', errorHandler);
});
