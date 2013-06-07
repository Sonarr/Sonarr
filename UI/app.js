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

define('app', ['shared/modal/region'], function (ModalRegion) {

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
        Toolbar  : {},
        Messenger: {},
        Cells: {}

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
        General        : {},
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






