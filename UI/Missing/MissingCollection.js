"use strict";
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Missing.MissingCollection = Backbone.Collection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/missing',
        model     : NzbDrone.Series.EpisodeModel,
        comparator: function (model) {
            return model.get('airDate');
        }
    });
});