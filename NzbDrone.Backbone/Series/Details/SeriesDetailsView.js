define(['app', 'Quality/QualityProfileCollection', 'Series/Details/SeasonCompositeView', 'Series/Details/SeasonCollection'], function () {
    NzbDrone.Series.Details.SeriesDetailsView = Backbone.Marionette.CompositeView.extend({

        itemView: NzbDrone.Series.Details.SeasonCompositeView,
        itemViewContainer: '.x-series-seasons',
        template: 'Series/Details/SeriesDetailsTemplate',

        initialize: function () {
            this.collection = new NzbDrone.Series.Details.SeasonCollection();
            this.collection.fetch({data: { seriesId: this.model.get('id') }});
        }
    });
});