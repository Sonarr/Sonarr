'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell',
        'Episode/Layout'
    ], function (App, NzbDroneCell, EpisodeLayout) {
        return NzbDroneCell.extend({

            className: 'episode-title-cell',

            events: {
                'click': 'showDetails'
            },

            showDetails: function () {
                var view = new EpisodeLayout({ model: this.cellValue });
                App.modalRegion.show(view);
            },

            render: function () {
                var title = this.cellValue.get('title');

                if (!title || title === '') {
                    title = 'TBA';
                }

                this.$el.html(title);
                return this;
            }
        });
    });
