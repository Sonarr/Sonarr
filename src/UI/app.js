'use strict';
require.config({

    paths : {
        'backbone'                : 'JsLibraries/backbone',
        'moment'                  : 'JsLibraries/moment',
        'filesize'                : 'JsLibraries/filesize',
        'handlebars'              : 'Shared/Shims/handlebars',
        'handlebars.helpers'      : 'JsLibraries/handlebars.helpers',
        'bootstrap'               : 'JsLibraries/bootstrap',
        'bootstrap.tagsinput'     : 'JsLibraries/bootstrap.tagsinput',
        'backbone.deepmodel'      : 'JsLibraries/backbone.deep.model',
        'backbone.pageable'       : 'JsLibraries/backbone.pageable',
        'backbone.validation'     : 'JsLibraries/backbone.validation',
        'backbone.modelbinder'    : 'JsLibraries/backbone.modelbinder',
        'backbone.collectionview' : 'JsLibraries/backbone.collectionview',
        'backgrid'                : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator'      : 'JsLibraries/backbone.backgrid.paginator',
        'backgrid.selectall'      : 'JsLibraries/backbone.backgrid.selectall',
        'fullcalendar'            : 'JsLibraries/fullcalendar',
        'backstrech'              : 'JsLibraries/jquery.backstretch',
        'underscore'              : 'JsLibraries/lodash.underscore',
        'marionette'              : 'JsLibraries/backbone.marionette',
        'signalR'                 : 'JsLibraries/jquery.signalR',
        'jquery-ui'               : 'JsLibraries/jquery-ui',
        'jquery.knob'             : 'JsLibraries/jquery.knob',
        'jquery.easypiechart'     : 'JsLibraries/jquery.easypiechart',
        'jquery.dotdotdot'        : 'JsLibraries/jquery.dotdotdot',
        'messenger'               : 'JsLibraries/messenger',
        'jquery'                  : 'JsLibraries/jquery',
        'typeahead'               : 'JsLibraries/typeahead',
        'zero.clipboard'          : 'JsLibraries/zero.clipboard',
        'jdu'                     : 'JsLibraries/jdu',
        'libs'                    : 'JsLibraries/'
    },

    shim : {
        api                       : {
            deps : ['jquery']
        },
        jquery                    : {
            exports : '$'
        },
        messenger                 : {
            deps    : ['jquery'],
            exports : 'Messenger',
            init    : function() {
                window.Messenger.options = {
                    theme : 'flat'
                };
            }
        },
        signalR                   : {
            deps : ['jquery']
        },
        bootstrap                 : {
            deps : ['jquery']
        },
        'bootstrap.tagsinput'     : {
            deps : [
                'bootstrap',
                'typeahead'
            ]
        },
        backstrech                : {
            deps : ['jquery']
        },
        underscore                : {
            deps    : ['jquery'],
            exports : '_'
        },
        backbone                  : {
            deps    : [
                'jquery',
                'Instrumentation/ErrorHandler',
                'underscore',
                'Mixins/jquery.ajax',
                'jQuery/ToTheTop'
            ],
            exports : 'Backbone'
        },
        marionette                : {
            deps    : [
                'backbone',
                'Handlebars/backbone.marionette.templates',
                'Mixins/AsNamedView'
            ],
            exports : 'Marionette',
            init    : function(Backbone, TemplateMixin, AsNamedView) {
                TemplateMixin.call(window.Marionette.TemplateCache);
                AsNamedView.call(window.Marionette.ItemView.prototype);
            }
        },
        'typeahead'               : {
            deps : ['jquery']
        },
        'jquery-ui'               : {
            deps : ['jquery']
        },
        'jquery.knob'             : {
            deps : ['jquery']
        },
        'jquery.easypiechart'     : {
            deps : ['jquery']
        },
        'jquery.dotdotdot'        : {
            deps : ['jquery']
        },
        'backbone.pageable'       : {
            deps : ['backbone']
        },
        'backbone.deepmodel'      : {
            deps : [
                'backbone',
                'underscore'
            ]
        },
        'backbone.validation'     : {
            deps    : ['backbone'],
            exports : 'Backbone.Validation'
        },
        'backbone.modelbinder'    : {
            deps : ['backbone']
        },
        'backbone.collectionview' : {
            deps    : [
                'backbone',
                'jquery-ui'
            ],
            exports : 'Backbone.CollectionView'
        },
        backgrid                  : {
            deps    : ['backbone'],
            exports : 'Backgrid',
            init    : function() {
                require(['Shared/Grid/HeaderCell'], function() {
                    window.Backgrid.Column.prototype.defaults = {
                        name       : undefined,
                        label      : undefined,
                        sortable   : true,
                        editable   : false,
                        renderable : true,
                        formatter  : undefined,
                        cell       : undefined,
                        headerCell : 'NzbDrone',
                        sortType   : 'toggle'
                    };
                });
            }
        },
        'backgrid.paginator'      : {
            deps    : ['backgrid'],
            exports : 'Backgrid.Extension.Paginator'
        },
        'backgrid.selectall'      : {
            deps    : ['backgrid'],
            exports : 'Backgrid.Extension.SelectRowCell'
        }
    }
});
