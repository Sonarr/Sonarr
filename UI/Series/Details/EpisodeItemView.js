'use strict';
define(['app', 'Series/SeasonModel'], function () {

    NzbDrone.Series.Details.EpisodeItemView = Backbone.Marionette.ItemView.extend({
        template: 'Series/Details/EpisodeItemTemplate',
        tagName : 'tr',

        ui: {

        },

        events: {

        }
    });
});
