/// <reference path="../app.js" />

NzbDrone.AddSeries.SearchResultModel = Backbone.Model.extend({

});

NzbDrone.AddSeries.SearchResultCollection = Backbone.Collection.extend({
    model: NzbDrone.AddSeries.SearchResultModel,
    url: "http://localhost/api/v1/series/lookup"
});

