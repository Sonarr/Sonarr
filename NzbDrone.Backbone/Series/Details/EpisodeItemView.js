'use strict';
define(['app', 'Series/Details/SeasonModel'], function () {

    NzbDrone.Series.Details.EpisodeItemView = Backbone.Marionette.ItemView.extend({
        template: 'Series/Details/EpisodeItemTemplate',
        tagName: 'tr',

        ui: {

        },

        events: {

        },
        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
