const path = require('path');
const webpack = require('webpack');
const FileManagerPlugin = require('filemanager-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const LiveReloadPlugin = require('webpack-livereload-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = (env) => {
  const uiFolder = 'UI';
  const frontendFolder = path.join(__dirname, '..');
  const srcFolder = path.join(frontendFolder, 'src');
  const isProduction = !!env.production;
  const isProfiling = isProduction && !!env.profile;
  const inlineWebWorkers = 'no-fallback';

  const distFolder = path.resolve(frontendFolder, '..', '_output', uiFolder);

  console.log('Source Folder:', srcFolder);
  console.log('Output Folder:', distFolder);
  console.log('isProduction:', isProduction);
  console.log('isProfiling:', isProfiling);

  const config = {
    mode: isProduction ? 'production' : 'development',
    devtool: isProduction ? 'source-map' : 'eval-source-map',

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
        jquery: 'jquery/src/jquery',
        'react-middle-truncate': 'react-middle-truncate/lib/react-middle-truncate'
      },
      fallback: {
        buffer: false,
        http: false,
        https: false,
        url: false,
        util: false,
        net: false
      }
    },

    output: {
      path: distFolder,
      publicPath: '/',
      filename: '[name].js',
      sourceMapFilename: '[file].map'
    },

    optimization: {
      moduleIds: 'deterministic',
      chunkIds: 'named',
      splitChunks: {
        chunks: 'initial',
        name: 'vendors'
      }
    },

    performance: {
      hints: false
    },

    plugins: [
      new webpack.DefinePlugin({
        __DEV__: !isProduction,
        'process.env.NODE_ENV': isProduction ? JSON.stringify('production') : JSON.stringify('development')
      }),

      new MiniCssExtractPlugin({
        filename: 'Content/styles.css'
      }),

      new HtmlWebpackPlugin({
        template: 'frontend/src/index.ejs',
        filename: 'index.html',
        publicPath: '/'
      }),

      new FileManagerPlugin({
        events: {
          onEnd: {
            copy: [
              // HTML
              {
                source: 'frontend/src/*.html',
                destination: distFolder
              },

              // Fonts
              {
                source: 'frontend/src/Content/Fonts/*.*',
                destination: path.join(distFolder, 'Content/Fonts')
              },

              // Icon Images
              {
                source: 'frontend/src/Content/Images/Icons/*.*',
                destination: path.join(distFolder, 'Content/Images/Icons')
              },

              // Images
              {
                source: 'frontend/src/Content/Images/*.*',
                destination: path.join(distFolder, 'Content/Images')
              },

              // Robots
              {
                source: 'frontend/src/Content/robots.txt',
                destination: path.join(distFolder, 'Content/robots.txt')
              }
            ]
          }
        }
      }),

      new LiveReloadPlugin()
    ],

    resolveLoader: {
      modules: [
        'node_modules',
        'frontend/build/webpack/'
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
                postcssOptions: {
                  config: 'frontend/postcss.config.js'
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

  return config;
};
