"use strict";
define(['app', 'AddSeries/SearchResultModel'], function () {
    NzbDrone.AddSeries.SearchResultCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series/lookup',
        model: NzbDrone.AddSeries.SearchResultModel
    });
});



