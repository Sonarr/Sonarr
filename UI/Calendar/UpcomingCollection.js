'use strict';
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Calendar.UpcomingCollection = Backbone.Collection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/calendar',
        model     : NzbDrone.Series.EpisodeModel,

        comparator: function(model) {
            var date = new Date(model.get('airDate'));
            var time = date.getTime();
            return time;
        }
    });
});
