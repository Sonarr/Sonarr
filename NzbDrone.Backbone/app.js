require.config({

    paths: {
        'backbone': 'JsLibraries/backbone',
        'underscore': 'JsLibraries/underscore',
        'marionette': 'JsLibraries/backbone.marionette',
        'handlebars': 'JsLibraries/handlebars'
    },

    shim: {
        underscore: {
            exports: '_'
        },
        backbone: {
            deps: ['underscore'],
            exports: 'Backbone'
        },
        marionette: {
            deps: ['backbone'],
            exports: 'Marionette'
        },
        handlebars: {
            exports: 'Handlebars'
        }

    }
});

define('app',  function () {

        window.NzbDrone = new Backbone.Marionette.Application();
        window.NzbDrone.Series = {};
        window.NzbDrone.Series.Edit = {};
        window.NzbDrone.Series.Delete = {};
        window.NzbDrone.AddSeries = {};
        window.NzbDrone.AddSeries.New = {};
        window.NzbDrone.AddSeries.Existing = {};
        window.NzbDrone.AddSeries.RootFolders = {};
        window.NzbDrone.Quality = {};
        window.NzbDrone.Shared = {};
        window.NzbDrone.Upcoming = {};
        window.NzbDrone.Calendar = {};

        window.NzbDrone.Events = {
            OpenModalDialog :'openModal',
            CloseModalDialog : 'closeModal'
        },

        window.NzbDrone.Constants = {
            ApiRoot: '/api'
        };

        window.NzbDrone.addInitializer(function () {

            console.log('starting application');

            NzbDrone.ModelBinder = new Backbone.ModelBinder();

            //TODO: move this out of here
            Handlebars.registerHelper("formatStatus", function (status, monitored) {
                if (!monitored) return '<i class="icon-pause grid-icon" title="Not Monitored"></i>';
                if (status === 'Continuing') return '<i class="icon-play grid-icon" title="Continuing"></i>';

                return '<i class="icon-stop grid-icon" title="Ended"></i>';
            });

            NzbDrone.addRegions({
                titleRegion: '#title-region',
                mainRegion: '#main-region',
                notificationRegion: '#notification-region',
            });
        });

        window.NzbDrone.start();

        return NzbDrone;
    });





