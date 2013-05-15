"use strict";

define(['app', 'Episode/Layout'], function () {
    NzbDrone.Series.Details.EpisodeDetailCell = Backgrid.Cell.extend({

        events: {
            'click': 'showDetails'
        },
        render: function () {
            this.$el.empty();
            this.$el.html('<i class="icon-ellipsis-vertical x-detail-icon"/>');
            return this;
        },

        showDetails: function () {
            var view = new NzbDrone.Episode.Layout({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });
});
