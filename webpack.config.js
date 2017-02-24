var path = require('path');
var webpack = require('webpack');

var uglifyJsPlugin = new webpack.optimize.UglifyJsPlugin();

var uiFolder = 'UI';
var root = path.join(__dirname, 'src', uiFolder);

module.exports = {
  devtool : '#source-map',
  watchOptions : { poll: true },
  entry: {
    vendor: 'vendor.js',
    main: 'main.js'
  },
  resolve: {
    root: root,
    alias: {
      'jdu': 'JsLibraries/jdu',
      'libs': 'JsLibraries/'
    }
  },
  output: {
    filename: '_output/' + uiFolder + '/[name].js',
    sourceMapFilename: '_output/' + uiFolder + '/[name].map'
  },
  plugins: [
    new webpack.optimize.CommonsChunkPlugin({ name: 'vendor' })
  ],
  module: {

    //this doesn't work yet. waiting for https://github.com/spenceralger/rcloader/issues/5
    /*preLoaders: [
        {
            test: /\.js$/, // include .js files
            loader: "jshint-loader",
            exclude: [/JsLibraries/,/node_modules/]
        }
    ]
    */
  }
};
