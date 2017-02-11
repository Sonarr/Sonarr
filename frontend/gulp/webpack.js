const _ = require('lodash');
const gulp = require('gulp');
const simpleVars = require('postcss-simple-vars');
const nested = require('postcss-nested');
const autoprefixer = require('autoprefixer');
const webpackStream = require('webpack-stream');
const livereload = require('gulp-livereload');
const path = require('path');
const webpack = require('webpack');
const errorHandler = require('./helpers/errorHandler');
const reload = require('require-nocache')(module);
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const uiFolder = 'UI';
const root = path.join(__dirname, '..', 'src');
const isProduction = process.argv.indexOf('--production') > -1;

console.log('ROOT:', root);
console.log('isProduction:', isProduction);

const cssVariables = [
  '../src/Styles/Variables/colors',
  '../src/Styles/Variables/dimensions',
  '../src/Styles/Variables/fonts',
  '../src/Styles/Variables/animations'
].map(require.resolve);

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
    root: [
      root,
      path.join(root, 'Shims'),
      path.join(root, 'JsLibraries')
    ]
  },
  output: {
    filename: path.join('_output', uiFolder, '[name].js'),
    sourceMapFilename: path.join('_output', uiFolder, '[file].map')
  },
  plugins: [
    new ExtractTextPlugin(path.join('_output', uiFolder, 'Content', 'styles.css'), { allChunks: true }),
    new webpack.optimize.CommonsChunkPlugin({
      name: 'vendor'
    }),
    new webpack.DefinePlugin({
      __DEV__: !isProduction,
      'process.env': {
        NODE_ENV: isProduction ? JSON.stringify('production') : JSON.stringify('development')
      }
    })
  ],
  resolveLoader: {
    modulesDirectories: [
      'node_modules',
      'gulp/webpack/'
    ]
  },
  eslint: {
    formatter: function(results) {
      return JSON.stringify(results);
    }
  },
  module: {
    loaders: [
      {
        test: /\.js?$/,
        exclude: /(node_modules|JsLibraries)/,
        loader: 'babel',
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
        loader: ExtractTextPlugin.extract('style', 'css-loader?modules&importLoaders=1&sourceMap&localIdentName=[name]__[local]___[hash:base64:5]!postcss-loader')
      },

      // Global styles
      {
        test: /\.css$/,
        include: /(node_modules|globals.css)/,
        loader: 'style!css-loader'
      },

      // Fonts
      {
        test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: 'url?limit=10240&mimetype=application/font-woff&emitFile=false&name=Content/Fonts/[name].[ext]'
      },
      {
        test: /\.(ttf|eot|eot?#iefix|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: 'file-loader?emitFile=false&name=Content/Fonts/[name].[ext]'
      }
    ]
  },
  postcss: function(wpack) {
    cssVariables.forEach(wpack.addDependency);

    return [
      simpleVars({
        variables: function() {
          return cssVariables.reduce(function(obj, vars) {
            return _.extend(obj, reload(vars));
          }, {});
        }
      }),
      nested(),
      autoprefixer({
        browsers: [
          'Chrome >= 30',
          'Firefox >= 30',
          'Safari >= 6',
          'Edge >= 12',
          'Explorer >= 10',
          'iOS >= 7',
          'Android >= 4.4'
        ]
      })
    ];
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
