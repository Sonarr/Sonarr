define(['app','Series/Details/SeasonModel'], function () {
    NzbDrone.Series.Details.SeasonCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/season'
    });
});
