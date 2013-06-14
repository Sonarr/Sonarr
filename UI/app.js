"use strict";
require.config({

    paths: {
        'backbone'  : 'JsLibraries/backbone',
        '$'         : 'JsLibraries/jquery',
        'underscore': 'JsLibraries/underscore',
        'marionette': 'JsLibraries/backbone.marionette',
        'handlebars': 'JsLibraries/handlebars',
        'libs'      : 'JsLibraries/'
    },

    shim: {
        underscore: {
            exports: '_'
        },
        backbone  : {
            deps   : ['underscore', '$'],
            exports: 'Backbone'
        },
        marionette: {
            deps   : ['backbone'],
            exports: 'Marionette'
        },
        handlebars: {
            exports: 'Handlebars'
        },

        backbone_backgrid :{
            exports: 'backgrid'
        },

        backgrid  : {
            deps: ['backbone', 'libs/backbone.backgrid', 'libs/backbone.backgrid.paginator']
        }
    }
});

define(['backbone','backgrid'], function (ModalRegion) {

    window.NzbDrone = new Backbone.Marionette.Application();
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






