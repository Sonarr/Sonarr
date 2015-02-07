var path = require('path');
var stylish = require('jshint-stylish');

module.exports = {
  entry     : 'main.js',
  resolve : {
  root : path.join(__dirname, "src", "ui"),
  alias : {
      'vent'                    : 'vent',
      'backbone'                : 'Shims/backbone',
      'moment'                  : 'JsLibraries/moment',
      'filesize'                : 'JsLibraries/filesize',
      'handlebars'              : 'Shims/handlebars',
      'handlebars.helpers'      : 'JsLibraries/handlebars.helpers',
      'bootstrap'               : 'JsLibraries/bootstrap',
      'backbone.deepmodel'      : 'JsLibraries/backbone.deep.model',
      'backbone.pageable'       : 'JsLibraries/backbone.pageable',
      'backbone-pageable'       : 'JsLibraries/backbone.pageable',
      'backbone.validation'     : 'Shims/backbone.validation',
      'backbone.modelbinder'    : 'JsLibraries/backbone.modelbinder',
      'backbone.collectionview' : 'Shims/backbone.collectionview',
      'backgrid'                : 'Shims/backgrid',
      'backgrid.paginator'      : 'Shims/backgrid.paginator',
      'backgrid.selectall'      : 'Shims/backbone.backgrid.selectall',
      'fullcalendar'            : 'JsLibraries/fullcalendar',
      'backstrech'              : 'JsLibraries/jquery.backstretch',
      'underscore'              : 'JsLibraries/lodash.underscore',
      'marionette'              : 'Shims/backbone.marionette',
      'signalR'                 : 'Shims/jquery.signalR',
      'jquery-ui'               : 'JsLibraries/jquery-ui',
      'jquery.knob'             : 'JsLibraries/jquery.knob',
      'jquery.easypiechart'     : 'JsLibraries/jquery.easypiechart',
      'jquery.dotdotdot'        : 'JsLibraries/jquery.dotdotdot',
      'messenger'               : 'Shims/messenger',
      'jquery'                  : 'Shims/jquery',
      'typeahead'               : 'JsLibraries/typeahead',
      'zero.clipboard'          : 'JsLibraries/zero.clipboard',
      'bootstrap.tagsinput'     : 'JsLibraries/bootstrap.tagsinput',
      'libs'                    : 'JsLibraries/'
    }
  },
  output : {
    filename : '_output/UI/main.js',
    sourceMapFilename : '_output/UI/main.map'
  },
  module: {
       //this doesn't work yet. wainting for https://github.com/spenceralger/rcloader/issues/5
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
