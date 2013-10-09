'use strict';
require.config({
    paths: {
        'backbone'            : 'JsLibraries/backbone',
        'moment'              : 'JsLibraries/moment',
        'filesize'            : 'JsLibraries/filesize',
        'handlebars'          : 'JsLibraries/handlebars.runtime',
        'handlebars.helpers'  : 'JsLibraries/handlebars.helpers',
        'bootstrap'           : 'JsLibraries/bootstrap',
        'backbone.deepmodel'  : 'JsLibraries/backbone.deep.model',
        'backbone.pageable'   : 'JsLibraries/backbone.pageable',
        'backbone.validation' : 'JsLibraries/backbone.validation',
        'backbone.modelbinder': 'JsLibraries/backbone.modelbinder',
        'backgrid'            : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator'  : 'JsLibraries/backbone.backgrid.paginator',
        'backgrid.selectall'  : 'JsLibraries/backbone.backgrid.selectall',
        'fullcalendar'        : 'JsLibraries/fullcalendar',
        'backstrech'          : 'JsLibraries/jquery.backstretch',
        'underscore'          : 'JsLibraries/lodash.underscore',
        'marionette'          : 'JsLibraries/backbone.marionette',
        'signalR'             : 'JsLibraries/jquery.signalR',
        'jquery.knob'         : 'JsLibraries/jquery.knob',
        'jquery.dotdotdot'    : 'JsLibraries/jquery.dotdotdot',
        'jquery'              : 'JsLibraries/jquery',
        'libs'                : 'JsLibraries/',

        'api': 'Require/require.api'
    },

    shim: {

        jquery: {
            exports: 'jQuery',
            deps   :
                [
                    'Mixins/jquery.ajax'
                ],

            init: function (AjaxMixin) {
                require(
                    [
                        'jQuery/ToTheTop',
                        'Instrumentation/ErrorHandler'
                    ]);

                AjaxMixin.apply($);
            }

        },

        signalR: {
            deps:
                [
                    'jquery'
                ]
        },

        bootstrap: {
            deps:
                [
                    'jquery'
                ],
            init: function ($) {
                $('body').tooltip({
                    selector: '[title]'
                });
            }
        },

        backstrech: {
            deps:
                [
                    'jquery'
                ]
        },

        underscore: {
            deps   :
                [
                    'jquery'
                ],
            exports: '_'
        },

        backbone: {
            deps   :
                [
                    'underscore',
                    'jquery'
                ],
            exports: 'Backbone'
        },


        'backbone.deepmodel': {
            deps:
                [
                    'Mixins/underscore.mixin.deepExtend'
                ]
        },

        'backbone.validation': {
            deps   :
                [
                    'backbone'
                ],
            exports: 'Backbone.Validation'
        },

        marionette: {
            deps:
                [
                    'backbone',
                    'Handlebars/backbone.marionette.templates',
                    'Mixins/AsNamedView'
                ],

            exports: 'Marionette',
            init   : function (Backbone, TemplateMixin, AsNamedView) {
                TemplateMixin.call(Marionette.TemplateCache);
                AsNamedView.call(Marionette.ItemView.prototype);

            }
        },

        'jquery.knob': {
            deps:
                [
                    'jquery'
                ]
        },

        'jquery.dotdotdot': {
            deps:
                [
                    'jquery'
                ]
        },

        'backbone.pageable': {
            deps:
                [
                    'backbone'
                ]
        },

        backgrid            : {
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

                        Backgrid.Column.prototype.defaults = {
                            name      : undefined,
                            label     : undefined,
                            sortable  : true,
                            editable  : false,
                            renderable: true,
                            formatter : undefined,
                            cell      : undefined,
                            headerCell: 'NzbDrone'
                        };

                    });
            }
        },
        'backgrid.paginator': {

            exports: 'Backgrid.Extension.Paginator',

            deps:
                [
                    'backgrid'
                ]
        },
        'backgrid.selectall': {

            exports: 'Backgrid.Extension.SelectAll',

            deps:
                [
                    'backgrid'
                ]
        }
    }
});

require.config({
    urlArgs: 'v=' + window.NzbDrone.ServerStatus.version
});

define(
    [
        'backbone',
        'marionette',
        'jQuery/RouteBinder',
        'Shared/SignalRBroadcaster',
        'Navbar/NavbarView',
        'AppLayout',
        'Series/SeriesController',
        'Router',
        'Shared/Modal/Controller',
        'Instrumentation/StringFormat'
    ], function (Backbone, Marionette, RouteBinder, SignalRBroadcaster, NavbarView, AppLayout, SeriesController, Router, ModalController) {

        new SeriesController();
        new ModalController();
        new Router();

        var app = new Marionette.Application();

        app.addInitializer(function () {
            console.log('starting application');
        });

        app.addInitializer(SignalRBroadcaster.appInitializer, {
            app: app
        });

        app.addInitializer(function () {
            Backbone.history.start({ pushState: true });
            RouteBinder.bind();
            AppLayout.navbarRegion.show(new NavbarView());
            $('body').addClass('started');
        });

        app.start();
    });
