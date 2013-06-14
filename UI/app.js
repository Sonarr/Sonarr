"use strict";
require.config({

    paths: {
        'backbone'          : 'JsLibraries/backbone',
        'handlebars'        : 'JsLibraries/handlebars.runtime',
        'bootstrap'         : 'JsLibraries/bootstrap',
        'bootstrap.slider'  : 'JsLibraries/bootstrap.slider',
        'backbone.mutators' : 'JsLibraries/backbone.mutators',
        'backbone.deepmodel': 'JsLibraries/backbone.deep.model',
        'backbone.pageable' : 'JsLibraries/backbone.pageable',
        'backgrid'          : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator': 'JsLibraries/backbone.backgrid.paginator',
        'fullcalendar'      : 'JsLibraries/fullcalendar',
        'backstrech'        : 'JsLibraries/jquery.backstretch',
        '$'                 : 'JsLibraries/jquery',
        'underscore'        : 'JsLibraries/underscore',
        'marionette'        : 'JsLibraries/backbone.marionette',
        'signalR'           : 'JsLibraries/jquery.signalR',
        'libs'              : 'JsLibraries/'
    },

    shim: {

        $: {
            exports: '$'
        },

        bootstrap: {
            deps: ['$']
        },

        'bootstrap.slider': {
            deps: ['$']
        },

        backstrech: {
            deps: ['$']
        },

        'underscore': {
            dep    : ['$'],
            exports: '_',
            init   : function () {
                require(['mixins/underscore.mixin.deepExtend']);
            }
        },

        backbone: {
            deps   : ['underscore', '$'],
            exports: 'Backbone',
            init   : function () {
                require(['libs/backbone.mutators']);
            }
        },

        marionette: {
            deps   : ['backbone'],
            exports: 'Marionette',
            init   : function () {
                require(['mixins/backbone.marionette.templates']);
            }
        },

        signalR: {
            dep: ['$']
        },

        'backbone.pageable': {
            dep : ['backbone'],
            init: function () {
                console.log(this);
            }
        },

        backgrid            : {
            deps: ['backbone'],
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
            deps: ['backgrid']
        }
    }
});

define([
    'marionette',
    'shared/modal/region',
    'Instrumentation/StringFormat',
    'Instrumentation/ErrorHandler'
], function (Marionette, ModalRegion) {

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

    window.NzbDrone.Settings = {
        Naming        : {},
        Quality       : {
            Size   : {},
            Profile: {}
        },
        Indexers      : {},
        DownloadClient: {},
        Notifications : {},
        General       : {},
        Misc          : {}
    };

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
        modalRegion       : ModalRegion
    });

    window.NzbDrone.start();

    return NzbDrone;
});






