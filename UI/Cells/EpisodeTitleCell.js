'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell'
    ], function (App, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'episode-title-cell',

            events: {
                'click': 'showDetails'
            },

            showDetails: function () {
                App.vent.trigger(App.Commands.ShowEpisodeDetails, {episode: this.cellValue});
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
