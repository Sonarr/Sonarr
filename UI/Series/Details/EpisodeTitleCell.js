"use strict";

define(['app', 'Episode/Layout'], function () {
    NzbDrone.Series.Details.EpisodeTitleCell = Backgrid.StringCell.extend({

        className: 'episode-title-cell',

        events: {
            'click': 'showDetails'
        },

        showDetails: function () {
            var view = new NzbDrone.Episode.Layout({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });
});
