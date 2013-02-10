/// <reference path="JsLibraries/jquery.js" />
/// <reference path="JsLibraries/underscore.js" />
/// <reference path="JsLibraries/sugar.js" />
/// <reference path="JsLibraries/backbone.js" />
/// <reference path="JsLibraries/handlebars.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/backbone.mutators.js" />
/// <reference path="JsLibraries/backbone.shortcuts.js" />
/// <reference path="JsLibraries/backbone.marionette.js" />
/// <reference path="JsLibraries/backbone.marionette.extend.js" />
/// <reference path="JsLibraries/backbone.marionette.viewswapper.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/bootstrap.js" />
/// <reference path="Shared/ModalRegion.js" />

if (typeof console === undefined) {
    window.console = { log: function () { } };
}

NzbDrone = new Backbone.Marionette.Application();
NzbDrone.Series = {};
NzbDrone.Series.Index = {};
NzbDrone.AddSeries = {};
NzbDrone.AddSeries.New = {};
NzbDrone.AddSeries.Existing = {};
NzbDrone.AddSeries.RootFolders = {};
NzbDrone.Quality = {};
NzbDrone.Shared = {};

/*
_.templateSettings = {
    interpolate: /\{\{([\s\S]+?)\}\}/g
};
*/

NzbDrone.ModelBinder = new Backbone.ModelBinder();

NzbDrone.Constants = {
    ApiRoot: '/api'
};

NzbDrone.Events = {
    DisplayInMainRegion: 'DisplayInMainRegion'
};

NzbDrone.Controller = Backbone.Marionette.Controller.extend({

    addSeries: function (action, query) {
        NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
    },

    series: function (action, query) {
        NzbDrone.mainRegion.show(new NzbDrone.Series.IndexLayout(this, action, query));
    },

    notFound: function () {
        alert('route not found');
    }
});

NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

    controller: new NzbDrone.Controller(),
    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        'series/index': 'series',
        'series/add': 'addSeries',
        'series/add/:action(/:query)': 'addSeries',
        ':whatever': 'notFound'
    }
});

NzbDrone.addInitializer(function (options) {

    console.log('starting application');

    NzbDrone.registerHelpers();

    NzbDrone.addRegions({
        mainRegion: '#main-region',
        notificationRegion: '#notification-region',
        modalRegion: ModalRegion
    });

    NzbDrone.Router = new NzbDrone.Router();
    Backbone.history.start();
});

NzbDrone.registerHelpers = function() {
    Handlebars.registerHelper("formatStatus", function (status, monitored) {
        if (!monitored) return '<i class="icon-pause grid-icon" title="Not Monitored"></i>';
        if (status === 'Continuing') return '<i class="icon-play grid-icon" title="Continuing"></i>';

        return '<i class="icon-stop grid-icon" title="Ended"></i>';
    });

    Handlebars.registerHelper("formatBestDate", function (dateSource) {
        if (!dateSource) return '';

        var date = Date.create(dateSource);

        if (date.isYesterday()) return 'Yesterday';
        if (date.isToday()) return 'Today';
        if (date.isTomorrow()) return 'Tomorrow';
        if (date.isToday()) return 'Today';
        if (date.isBefore(Date.create().addDays(7))) return date.format('{Weekday}');

        return date.format('{MM}/{dd}/{yyyy}');
    });

    Handlebars.registerHelper("formatProgress", function (episodeFileCount, episodeCount) {
        var percent = 100;

        if (!episodeFileCount) episodeFileCount = 0;
        if (!episodeCount) episodeCount = 0;

        if (episodeCount > 0)
            percent = episodeFileCount / episodeCount * 100;

        var result = '<div class="progress">';
        result += '<span class="progressbar-back-text">' + episodeFileCount + ' / ' + episodeCount + '</span>';
        result += '<div class="bar" style="width: ' + percent + '%"><span class="progressbar-front-text">' + episodeFileCount + ' / ' + episodeCount + '</span></div>';
        return result + '</div>';
    });
}