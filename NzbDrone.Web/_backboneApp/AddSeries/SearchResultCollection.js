/// <reference path="../app.js" />
/// <reference path="SearchResultModel.js" />
"use strict";

NzbDrone.AddSeries.SearchResultCollection = Backbone.Collection.extend({
    url: NzbDrone.Constants.ApiRoot + '/series/lookup',
    model: NzbDrone.AddSeries.SearchResultModel

});

