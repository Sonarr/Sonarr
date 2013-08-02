'use strict';
require.config({

    urlArgs: 'v=' + window.ServerStatus.version,

    paths: {
        'backbone'            : 'JsLibraries/backbone',
        'moment'              : 'JsLibraries/moment',
        'filesize'            : 'JsLibraries/filesize',
        'handlebars'          : 'JsLibraries/handlebars.runtime',
        'handlebars.helpers'  : 'JsLibraries/handlebars.helpers',
        'bootstrap'           : 'JsLibraries/bootstrap',
        'backbone.deepmodel'  : 'JsLibraries/backbone.deep.model',
        'backbone.pageable'   : 'JsLibraries/backbone.pageable',
        'backbone.modelbinder': 'JsLibraries/backbone.modelbinder',
        'backgrid'            : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator'  : 'JsLibraries/backbone.backgrid.paginator',
        'fullcalendar'        : 'JsLibraries/fullcalendar',
        'backstrech'          : 'JsLibraries/jquery.backstretch',
        '$'                   : 'JsLibraries/jquery',
        'underscore'          : 'JsLibraries/lodash.underscore',
        'marionette'          : 'JsLibraries/backbone.marionette',
        'signalR'             : 'JsLibraries/jquery.signalR',
        'jquery.knob'         : 'JsLibraries/jquery.knob',
        'libs'                : 'JsLibraries/'

    },

    shim: {

        $: {
            exports: '$',

            init: function () {
                require(
                    [
                        'jQuery/ToTheTop',
                        'Instrumentation/ErrorHandler'
                    ]);
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
                    'Mixins/backbone.ajax',
                    'underscore',
                    '$'
                ],
            exports: 'Backbone',
            init   : function (AjaxMixin) {
                AjaxMixin.apply(Backbone);
            }
        },


        'backbone.deepmodel': {
            deps:
                [
                    'Mixins/underscore.mixin.deepExtend'
                ]
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
        }
    }
});

define(
    [
        'marionette',
        'Instrumentation/StringFormat'
    ], function (Marionette) {

        var app = new Marionette.Application();

        app.Events = {
            SeriesAdded  : 'series:added',
            SeriesDeleted: 'series:deleted'
        };

        app.Commands = {
            EditSeriesCommand  : 'EditSeriesCommand',
            DeleteSeriesCommand: 'DeleteSeriesCommand',
            CloseModalCommand  : 'CloseModalCommand',
            ShowEpisodeDetails : 'ShowEpisodeDetails',
            SaveSettings       : 'saveSettings',
            ShowLogFile        : 'showLogFile'
        };

        app.addInitializer(function () {
            console.log('starting application');
        });

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
