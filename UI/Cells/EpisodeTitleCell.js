'use strict';

define(['app', 'Cells/NzbDroneCell'], function () {
    NzbDrone.Cells.EpisodeTitleCell = NzbDrone.Cells.NzbDroneCell.extend({

        className: 'episode-title-cell',

        events: {
            'click': 'showDetails'
        },

        showDetails: function () {
            var view = new NzbDrone.Episode.Layout({ model: this.cellValue });
            NzbDrone.modalRegion.show(view);
        },

        render: function () {
            this.$el.html(this.cellValue.get('title'));
            return this;
        }
    });
});
