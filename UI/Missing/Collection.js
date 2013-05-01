"use strict";
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Missing.Collection = Backbone.PageableCollection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/missing',
        model     : NzbDrone.Series.EpisodeModel,
        comparator: function (model) {
            return model.get('airDate');
        }
    });
});