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
    window.NzbDrone.Series = {};
    window.NzbDrone.Series.Index = {};
    window.NzbDrone.Series.Index.Table = {};
    window.NzbDrone.Series.Index.List = {};
    window.NzbDrone.Series.Index.Posters = {};
    window.NzbDrone.Series.Edit = {};
    window.NzbDrone.Series.Delete = {};
    window.NzbDrone.Series.Details = {};
    window.NzbDrone.AddSeries = {};
    window.NzbDrone.AddSeries.New = {};
    window.NzbDrone.AddSeries.Existing = {};
    window.NzbDrone.AddSeries.RootFolders = {};
    window.NzbDrone.Quality = {};
    window.NzbDrone.Shared = {};
    window.NzbDrone.Shared.Toolbar = {};
    window.NzbDrone.Upcoming = {};
    window.NzbDrone.Calendar = {};
    window.NzbDrone.Settings = {};
    window.NzbDrone.Settings.Naming = {};
    window.NzbDrone.Settings.Quality = {};
    window.NzbDrone.Settings.Quality.Size = {};
    window.NzbDrone.Settings.Quality.Profile = {};
    window.NzbDrone.Settings.Indexers = {};
    window.NzbDrone.Settings.DownloadClient = {};
    window.NzbDrone.Settings.Notifications = {};
    window.NzbDrone.Settings.System = {};
    window.NzbDrone.Settings.Misc = {};
    window.NzbDrone.Missing = {};
    window.NzbDrone.History = {};

    window.NzbDrone.Events = {
        //TODO: Move to commands
        OpenModalDialog : 'openModal',
        CloseModalDialog: 'closeModal',
        SeriesAdded: 'seriesAdded'
    };

    window.NzbDrone.Commands = {
        SaveSettings : 'saveSettings'
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






