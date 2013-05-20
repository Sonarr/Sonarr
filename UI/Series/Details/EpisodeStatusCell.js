"use strict";

define(['app', 'Episode/Layout'], function () {
    NzbDrone.Series.Details.EpisodeStatusCell = Backgrid.Cell.extend({

        events: {
            'click': 'showDetails'
        },
        render: function () {
            this.$el.empty();

            if (this.model) {

                var icon;

                if (this.model.get('episodeFile')) {
                    icon = 'icon-ok';

                }

                this.$el.html('<i class="{0}"/>'.format(icon));
            }

            return this;
        },

        showDetails: function () {
            var view = new NzbDrone.Episode.Layout({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });
});
