"use strict";
require.config({

    urlArgs: 'bust=' + window.ServerStatus.version,

    paths: {
        'backbone'            : 'JsLibraries/backbone',
        'sugar'               : 'JsLibraries/sugar',
        'handlebars'          : 'JsLibraries/handlebars.runtime',
        'bootstrap'           : 'JsLibraries/bootstrap',
        'bootstrap.slider'    : 'JsLibraries/bootstrap.slider',
        'backbone.mutators'   : 'JsLibraries/backbone.mutators',
        'backbone.deepmodel'  : 'JsLibraries/backbone.deep.model',
        'backbone.pageable'   : 'JsLibraries/backbone.pageable',
        'backbone.modelbinder': 'JsLibraries/backbone.modelbinder',
        'backgrid'            : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator'  : 'JsLibraries/backbone.backgrid.paginator',
        'fullcalendar'        : 'JsLibraries/fullcalendar',
        'backstrech'          : 'JsLibraries/jquery.backstretch',
        '$'                   : 'JsLibraries/jquery',
        'underscore'          : 'JsLibraries/underscore',
        'marionette'          : 'JsLibraries/backbone.marionette',
        'signalR'             : 'JsLibraries/jquery.signalR',
        'libs'                : 'JsLibraries/'
    },

    shim: {

        $: {
            exports: '$',
            init   : function () {
                window.Constants = {
                    ApiRoot: '/api'
                };
            }
        },

        bootstrap: {
            deps:
                [
                    '$'
                ]
        },

        'bootstrap.slider': {
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

        'underscore': {
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
                    'mixins/underscore.mixin.deepExtend'
                ]
        },

        marionette: {
            deps:
                [
                    'backbone',
                    'handlebars',
                    'mixins/backbone.marionette.templates',
                    'mixins/AsNamedView',

                    'Handlebars/Helpers/DateTime'
                ],

            exports: 'Marionette',
            init   : function (Backbone, Handlebars, TemplateMixin, AsNamedView, DateTimeHelpers) {
                TemplateMixin.call(Marionette.TemplateCache);
                AsNamedView.call(Marionette.ItemView.prototype);
                DateTimeHelpers.register(Handlebars);

            }
        },


        signalR: {
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
            init: function () {
                Backgrid.Column.prototype.defaults = {
                    name      : undefined,
                    label     : undefined,
                    sortable  : true,
                    editable  : false,
                    renderable: true,
                    formatter : undefined,
                    cell      : undefined,
                    headerCell: 'nzbDrone'
                };
            }
        },
        'backgrid.paginator': {
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
        'shared/modal/region',
        'Instrumentation/StringFormat',
        'Instrumentation/ErrorHandler'
    ], function (Marionette, ModalRegion) {

        require(
            [
                'libs/backbone.mutators'
            ]);


        window.NzbDrone = new Marionette.Application();
        window.NzbDrone.Config = {};
        window.NzbDrone.Form = {};

        window.NzbDrone.Series = {
            Index  : {
                Table  : {},
                List   : {},
                Posters: {}

            },
            Edit   : {},
            Delete : {},
            Details: {}
        };

        window.NzbDrone.AddSeries = {
            New        : {},
            Existing   : {},
            RootFolders: {}
        };

        window.NzbDrone.Episode = {
            Search  : {},
            Summary : {},
            Activity: {}
        };

        window.NzbDrone.Quality = {};

        window.NzbDrone.Commands = {};

        window.NzbDrone.Shared = {
            Toolbar      : {},
            Messenger    : {},
            FormatHelpers: {},
            Grid         : {}
        };

        window.NzbDrone.Cells = {};

        window.NzbDrone.Calendar = {};

        window.NzbDrone.Missing = {};
        window.NzbDrone.History = {};
        window.NzbDrone.Logs = {};
        window.NzbDrone.Release = {};
        window.NzbDrone.Mixins = {};

        window.NzbDrone.Events = {
            SeriesAdded: 'seriesAdded'
        };

        window.NzbDrone.Commands = {
            SaveSettings: 'saveSettings'
        };

        window.NzbDrone.Constants = {
            ApiRoot: '/api'
        };

        window.NzbDrone.addInitializer(function () {

            console.log('starting application');

        });

        NzbDrone.addRegions({
            mainRegion        : '#main-region',
            notificationRegion: '#notification-region',
            modalRegion       : ModalRegion,
            footerRegion      : '#footer-region'
        });

        window.NzbDrone.start();


        window.require(
            [
                'Routing'
            ]);

        return NzbDrone;
    });






