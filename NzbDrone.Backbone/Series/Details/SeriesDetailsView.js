define(['app', 'Quality/QualityProfileCollection', 'Series/Details/SeasonCollectionView'], function (app, qualityProfileCollection) {
    NzbDrone.Series.Details.SeriesDetailsView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Series.Details.SeasonCollectionView,
        itemViewContainer: '#seasons',
        template: 'Series/Details/SeriesDetailsTemplate',
        qualityProfileCollection: qualityProfileCollection,

        initialize: function (options) {
            this.collection = options.collection;

            this.qualityProfileCollection.fetch();
        },

        onCompositeCollectionRendered: function()
        {
            var test = 1;
        }
    });
});