define(['app', 'Series/SeriesModel'], function () {
    NzbDrone.Series.SeriesCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series',
        model: NzbDrone.Series.SeriesModel,

        defaultSortKey: 'title',
        defaultSortDir: '-1',

        comparator: function (model) {
            return model.get(this.defaultSortKey);
        }
    });
});