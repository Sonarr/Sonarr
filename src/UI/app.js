'use strict';
require.config({

    urlArgs: 'v=' + window.NzbDrone.ServerStatus.version,

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
        'backgrid.selectall' : 'JsLibraries/backbone.backgrid.selectall',
        'fullcalendar'        : 'JsLibraries/fullcalendar',
        'backstrech'          : 'JsLibraries/jquery.backstretch',
        '$'                   : 'JsLibraries/jquery',
        'underscore'          : 'JsLibraries/lodash.underscore',
        'marionette'          : 'JsLibraries/backbone.marionette',
        'signalR'             : 'JsLibraries/jquery.signalR',
        'jquery.knob'         : 'JsLibraries/jquery.knob',
        'jquery.dotdotdot'    : 'JsLibraries/jquery.dotdotdot',
        'libs'                : 'JsLibraries/'
    },

    shim: {

        $: {
            exports: '$',

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
                    '$'
                ]
        },

        bootstrap: {
            deps:
                [
                    '$'
                ]
        },

        backstrech: {
            deps:
                [
                    '$'
                ]
        },

        underscore: {
            deps   :
                [
                    '$'
                ],
            exports: '_'
        },

        backbone: {
            deps   :
                [
                    'underscore',
                    '$'
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
                    '$'
                ]
        },

        'jquery.dotdotdot': {
            deps:
                [
                    '$'
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

define(
    [
        'marionette',
        'Shared/SignalRBroadcaster',
        'Instrumentation/StringFormat'
    ], function (Marionette, SignalRBroadcaster) {

        var app = new Marionette.Application();

        app.Events = {
            SeriesAdded    : 'series:added',
            SeriesDeleted  : 'series:deleted',
            SeasonRenamed  : 'season:renamed',
            CommandComplete: 'command:complete'
        };

        app.Commands = {
            EditSeriesCommand    : 'EditSeriesCommand',
            DeleteSeriesCommand  : 'DeleteSeriesCommand',
            CloseModalCommand    : 'CloseModalCommand',
            ShowEpisodeDetails   : 'ShowEpisodeDetails',
            ShowHistoryDetails   : 'ShowHistoryDetails',
            SaveSettings         : 'saveSettings',
            ShowLogFile          : 'showLogFile'
        };

        app.Reqres = {
            GetEpisodeFileById: 'GetEpisodeFileById'
        };

        app.addInitializer(function () {
            console.log('starting application');
        });

        app.addInitializer(SignalRBroadcaster.appInitializer, { app: app });

        app.addRegions({
            navbarRegion: '#nav-region',
            mainRegion  : '#main-region',
            footerRegion: '#footer-region'
        });

        app.start();

        window.require(
            [
                'Router',
                'jQuery/TooltipBinder'
            ]);

        return app;
    });
