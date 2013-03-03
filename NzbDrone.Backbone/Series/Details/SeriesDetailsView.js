define(['app', 'Quality/QualityProfileCollection', 'Series/Details/SeasonCompositeView'], function () {
    NzbDrone.Series.Details.SeriesDetailsView = Backbone.Marionette.CompositeView.extend({

        itemView: NzbDrone.Series.Details.SeasonCompositeView,
        itemViewContainer: '.x-series-seasons',
        template: 'Series/Details/SeriesDetailsTemplate',

        initialize: function () {
        }
    });
});