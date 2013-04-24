"use strict";
define(['app', 'Series/SeasonModel'], function () {
    NzbDrone.Series.SeasonCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/season',
        model: NzbDrone.Series.SeasonModel
    });
});
