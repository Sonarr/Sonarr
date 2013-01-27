/// <reference path="../app.js" />
/// <reference path="SearchResultModel.js" />

NzbDrone.AddSeries.SearchResultCollection = Backbone.Collection.extend({
    url: NzbDrone.Constants.ApiRoot + '/series/lookup',
    model: NzbDrone.AddSeries.SearchResultModel,
});

