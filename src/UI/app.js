'use strict';
require.config({

    paths: {
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
        'libs'                    : 'JsLibraries/',

        'api': 'Require/require.api'
    },

    shim: {

        api: {
            deps:
                [
                    'jquery'
                ]
        },

        jquery                : {
            exports: '$'
        },
        messenger             : {
            deps   :
                [
                    'jquery'
                ],
            exports: 'Messenger',
            init : function () {
                window.Messenger.options = {
                    theme: 'flat'
                };
            }
        },
        signalR               : {
            deps:
                [
                    'jquery'
                ]
        },
        bootstrap             : {
            deps:
                [
                    'jquery'
                ]
        },
        'bootstrap.tagsinput' : {
            deps:
                [
                    'bootstrap',
                    'typeahead'
                ]
        },
        backstrech            : {
            deps:
                [
                    'jquery'
                ]
        },
        underscore            : {
            deps   :
                [
                    'jquery'
                ],
            exports: '_'
        },
        backbone              : {
            deps:
                [
                    'jquery',
                    'Instrumentation/ErrorHandler',
                    'underscore',
                    'Mixins/jquery.ajax',
                    'jQuery/ToTheTop'
                ],

            exports: 'Backbone'
        },
        marionette            : {
            deps:
                [
                    'backbone',
                    'Handlebars/backbone.marionette.templates',
                    'Mixins/AsNamedView'
                ],

            exports: 'Marionette',
            init   : function (Backbone, TemplateMixin, AsNamedView) {
                TemplateMixin.call(window.Marionette.TemplateCache);
                AsNamedView.call(window.Marionette.ItemView.prototype);

            }
        },
        'typeahead'           : {
            deps:
                [
                    'jquery'
                ]
        },
        'jquery-ui'           : {
            deps:
                [
                    'jquery'
                ]
        },
        'jquery.knob'         : {
            deps:
                [
                    'jquery'
                ]
        },
        'jquery.easypiechart'         : {
            deps:
                [
                    'jquery'
                ]
        },
        'jquery.dotdotdot'    : {
            deps:
                [
                    'jquery'
                ]
        },
        'backbone.pageable'   : {
            deps:
                [
                    'backbone'
                ]
        },
        'backbone.deepmodel'  : {
            deps:
                [
                    'backbone',
                    'underscore'
                ]
        },
        'backbone.validation' : {
            deps   :
                [
                    'backbone'
                ],
            exports: 'Backbone.Validation'
        },
        'backbone.modelbinder': {
            deps:
                [
                    'backbone'
                ]
        },
        'backbone.collectionview': {
            deps:
                [
                    'backbone',
                    'jquery-ui'
                ],                
            exports: 'Backbone.CollectionView'
        },
        backgrid              : {
            deps:
                [
                    'backbone'
                ],

            exports: 'Backgrid',

            init: function () {
                require(
                    [
                        'Shared/Grid/HeaderCell'
                    ], function () {

                        window.Backgrid.Column.prototype.defaults = {
                            name      : undefined,
                            label     : undefined,
                            sortable  : true,
                            editable  : false,
                            renderable: true,
                            formatter : undefined,
                            cell      : undefined,
                            headerCell: 'NzbDrone',
                            sortType  : 'toggle'
                        };
                    });
            }
        },
        'backgrid.paginator'  : {

            exports: 'Backgrid.Extension.Paginator',

            deps:
                [
                    'backgrid'
                ]
        },
        'backgrid.selectall'  : {

            exports: 'Backgrid.Extension.SelectAll',

            deps:
                [
                    'backgrid'
                ]
        }
    }
});


require.config({
    urlArgs: 'v=' + window.NzbDrone.Version
});

define(
    [
        'jquery',
        'backbone',
        'marionette',
        'jQuery/RouteBinder',
        'Shared/SignalRBroadcaster',
        'Navbar/NavbarLayout',
        'AppLayout',
        'Series/SeriesController',
        'Router',
        'Shared/Modal/ModalController',
        'Shared/ControlPanel/ControlPanelController',
        'System/StatusModel',
        'Shared/Tooltip',
        'Instrumentation/StringFormat',
        'LifeCycle',
        'Hotkeys/Hotkeys'
    ], function ($,
                 Backbone,
                 Marionette,
                 RouteBinder,
                 SignalRBroadcaster,
                 NavbarLayout,
                 AppLayout,
                 SeriesController,
                 Router,
                 ModalController,
                 ControlPanelController,
                 serverStatusModel,
                 Tooltip) {

        new SeriesController();
        new ModalController();
        new ControlPanelController();
        new Router();

        var app = new Marionette.Application();

        app.addInitializer(function () {
            console.log('starting application');
        });

        app.addInitializer(SignalRBroadcaster.appInitializer, {
            app: app
        });

        app.addInitializer(Tooltip.appInitializer, {
            app: app
        });

        app.addInitializer(function () {
            Backbone.history.start({ pushState: true, root: serverStatusModel.get('urlBase') });
            RouteBinder.bind();
            AppLayout.navbarRegion.show(new NavbarLayout());
            $('body').addClass('started');
        });

        app.addInitializer(function () {

            var footerText = serverStatusModel.get('version');

            if (serverStatusModel.get('branch') !== 'master') {
                footerText += '</br>' + serverStatusModel.get('branch');
            }

            $('#footer-region .version').html(footerText);
        });

        return app;
    });