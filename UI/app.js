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
    window.NzbDrone.Series.Index.List = {};
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

    window.NzbDrone.Events = {
        OpenModalDialog : 'openModal',
        CloseModalDialog: 'closeModal',
        SeriesAdded: 'seriesAdded'
    };

    window.NzbDrone.Constants = {
        ApiRoot: '/api'
    };

    window.NzbDrone.addInitializer(function () {

        console.log('starting application');

        //TODO: move these out of here
        Backgrid.SeriesStatusCell = Backgrid.Cell.extend({
            className: "series-status-cell",

            render: function () {
                this.$el.empty();
                var monitored = this.model.get('monitored');
                var status = this.model.get('status');

                if (!monitored) {
                    this.$el.html('<i class="icon-pause grid-icon" title="Not Monitored"></i>');
                }
                else if (status === 0) {
                    this.$el.html('<i class="icon-play grid-icon" title="Continuing"></i>');
                }

                else {
                    this.$el.html('<i class="icon-stop grid-icon" title="Ended"></i>');
                }

                return this;
            }
        });

        var AirDateFormatter = Backgrid.AirDateFormatter = function () {};
        AirDateFormatter.prototype = new Backgrid.CellFormatter();
        _.extend(AirDateFormatter.prototype, {
            /**
             Converts any value to a string using Ecmascript's implicit type
             conversion. If the given value is `null` or `undefined`, an empty string is
             returned instead.

             @member Backgrid.StringFormatter
             @param {*} rawValue
             @return {string}
             */
            fromRaw: function (rawValue) {
                return 'Hello World';

                if (_.isUndefined(rawValue) || _.isNull(rawValue)) return '';
                return rawValue + '';
            }
        });
    });

    NzbDrone.addRegions({
        titleRegion       : '#title-region',
        mainRegion        : '#main-region',
        notificationRegion: '#notification-region'
    });


    window.NzbDrone.start();

    return NzbDrone;
});






