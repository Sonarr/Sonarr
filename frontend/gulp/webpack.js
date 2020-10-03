const gulp = require('gulp');
const webpackStream = require('webpack-stream');
const livereload = require('gulp-livereload');
const path = require('path');
const webpack = require('webpack');
const errorHandler = require('./helpers/errorHandler');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const HtmlWebpackPluginHtmlTags = require('html-webpack-plugin/lib/html-tags');
const TerserPlugin = require('terser-webpack-plugin');

const uiFolder = 'UI';
const frontendFolder = path.join(__dirname, '..');
const srcFolder = path.join(frontendFolder, 'src');
const isProduction = process.argv.indexOf('--production') > -1;
const isProfiling = isProduction && process.argv.indexOf('--profile') > -1;
const inlineWebWorkers = 'no-fallback';

const distFolder = path.resolve(frontendFolder, '..', '_output', uiFolder);

console.log('Source Folder:', srcFolder);
console.log('Output Folder:', distFolder);
console.log('isProduction:', isProduction);
console.log('isProfiling:', isProfiling);

const cssVarsFiles = [
  '../src/Styles/Variables/colors',
  '../src/Styles/Variables/dimensions',
  '../src/Styles/Variables/fonts',
  '../src/Styles/Variables/animations',
  '../src/Styles/Variables/zIndexes'
].map(require.resolve);

// Override the way HtmlWebpackPlugin injects the scripts
// TODO: Find a better way to get these paths without
HtmlWebpackPlugin.prototype.injectAssetsIntoHtml = function(html, assets, assetTags) {
  const head = assetTags.headTags.map((v) => {
    const href = v.attributes.href
      .replace('\\', '/')
      .replace('%5C', '/');

    v.attributes = { rel: 'stylesheet', type: 'text/css', href: `/${href}` };
    return HtmlWebpackPluginHtmlTags.htmlTagObjectToString(v, this.options.xhtml);
  });
  const body = assetTags.bodyTags.map((v) => {
    v.attributes = { src: `/${v.attributes.src}` };
    return HtmlWebpackPluginHtmlTags.htmlTagObjectToString(v, this.options.xhtml);
  });

  return html
    .replace('<!-- webpack bundles head -->', head.join('\r\n  '))
    .replace('<!-- webpack bundles body -->', body.join('\r\n  '));
};

const plugins = [
  new webpack.DefinePlugin({
    __DEV__: !isProduction,
    'process.env.NODE_ENV': isProduction ? JSON.stringify('production') : JSON.stringify('development')
  }),

  new MiniCssExtractPlugin({
    filename: path.join('Content', 'styles.css')
  }),

  new HtmlWebpackPlugin({
    template: 'frontend/src/index.html',
    filename: 'index.html'
  })
];

const config = {
  mode: isProduction ? 'production' : 'development',
  devtool: '#source-map',

  stats: {
    children: false
  },

  watchOptions: {
    ignored: /node_modules/
  },

  entry: {
    index: 'index.js'
  },

  resolve: {
    modules: [
      srcFolder,
      path.join(srcFolder, 'Shims'),
      'node_modules'
    ],
    alias: {
      jquery: 'jquery/src/jquery'
    }
  },

  output: {
    path: distFolder,
    filename: '[name].js',
    sourceMapFilename: '[file].map'
  },

  optimization: {
    chunkIds: 'named',
    splitChunks: {
      chunks: 'initial'
    }
  },

  performance: {
    hints: false
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
        test: /\.worker\.js$/,
        use: {
          loader: 'worker-loader',
          options: {
            filename: '[name].js',
            inline: inlineWebWorkers
          }
        }
      },
      {
        test: /\.js?$/,
        exclude: /(node_modules|JsLibraries)/,
        use: [
          {
            loader: 'babel-loader',
            options: {
              configFile: `${frontendFolder}/babel.config.js`,
              envName: isProduction ? 'production' : 'development',
              presets: [
                [
                  '@babel/preset-env',
                  {
                    modules: false,
                    loose: true,
                    debug: false,
                    useBuiltIns: 'entry',
                    corejs: 3
                  }
                ]
              ]
            }
          }
        ]
      },

      // CSS Modules
      {
        test: /\.css$/,
        exclude: /(node_modules|globals.css)/,
        use: [
          { loader: MiniCssExtractPlugin.loader },
          {
            loader: 'css-loader',
            options: {
              importLoaders: 1,
              modules: {
                localIdentName: '[name]/[local]/[hash:base64:5]'
              }
            }
          },
          {
            loader: 'postcss-loader',
            options: {
              ident: 'postcss',
              config: {
                ctx: {
                  cssVarsFiles
                },
                path: 'frontend/postcss.config.js'
              }
            }
          }
        ]
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

if (isProfiling) {
  config.resolve.alias['react-dom$'] = 'react-dom/profiling';
  config.resolve.alias['scheduler/tracing'] = 'scheduler/tracing-profiling';

  config.optimization.minimizer = [
    new TerserPlugin({
      cache: true,
      parallel: true,
      sourceMap: true, // Must be set to true if using source-maps in production
      terserOptions: {
        mangle: false,
        keep_classnames: true,
        keep_fnames: true
      }
    })
  ];
}

gulp.task('webpack', () => {
  return webpackStream(config)
    .pipe(gulp.dest('_output/UI'));
});

gulp.task('webpackWatch', () => {
  config.watch = true;

  return webpackStream(config)
    .on('error', errorHandler)
    .pipe(gulp.dest('_output/UI'))
    .on('error', errorHandler)
    .pipe(livereload())
    .on('error', errorHandler);
});
