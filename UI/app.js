"use strict";
require.config({

    paths: {
        'backbone'  : 'JsLibraries/backbone',
        'underscore': 'JsLibraries/underscore',
        'marionette': 'JsLibraries/backbone.marionette',
        'handlebars': 'JsLibraries/handlebars'
    },

    shim: {
        underscore: {
            exports: '_'
        },
        backbone  : {
            deps   : ['underscore'],
            exports: 'Backbone'
        },
        marionette: {
            deps   : ['backbone'],
            exports: 'Marionette'
        },
        handlebars: {
            exports: 'Handlebars'
        }
    }
});

define('app', function () {

    window.NzbDrone = new Backbone.Marionette.Application();
    window.NzbDrone.Config = {};

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


    window.NzbDrone.Quality = {};

    window.NzbDrone.Commands = {};

    window.NzbDrone.Shared = {
        Toolbar  : {},
        Messenger: {}
    };
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
        System        : {},
        Misc          : {}
    };

    window.NzbDrone.Missing = {};
    window.NzbDrone.History = {};

    window.NzbDrone.Events = {
        //TODO: Move to commands
        OpenModalDialog : 'openModal',
        CloseModalDialog: 'closeModal',
        SeriesAdded     : 'seriesAdded'
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
        notificationRegion: '#notification-region'
    });

    window.NzbDrone.start();

    return NzbDrone;
});






