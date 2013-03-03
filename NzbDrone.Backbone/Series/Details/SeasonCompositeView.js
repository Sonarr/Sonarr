'use strict';
define(['app', 'Series/Details/EpisodeItemView'], function () {
    NzbDrone.Series.Details.SeasonCompositeView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Series.Details.EpisodeItemView,
        itemViewContainer: '.x-episodes',
        template: 'Series/Details/SeasonCompositeTemplate',

        initialize: function() {

        }
    });
});