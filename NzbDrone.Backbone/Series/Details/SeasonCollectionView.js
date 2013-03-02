'use strict';

define(['app', 'Series/Details/EpisodeItemView'], function (app) {
    NzbDrone.Series.Details.SeasonCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Series.Details.EpisodeItemView,
        itemViewContainer: '#seasons',
        template: 'Series/Details/SeasonCollectionTemplate',

        initialize: function() {
            var episodes = this.model.get('episodes');
            var test = 1;
            //this.collection
        }
    });
});