"use strict";
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Calendar.CalendarCollection = Backbone.Collection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/calendar',
        model     : NzbDrone.Series.EpisodeModel,
        comparator: function (model) {
            return model.get('airDate');
        }
    });
});