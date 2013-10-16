'use strict';

define(
    [
        'vent',
        'Cells/NzbDroneCell'
    ], function (vent, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'episode-title-cell',

            events: {
                'click': '_showDetails'
            },

            render: function () {
                var title = this.cellValue.get('title');

                if (!title || title === '') {
                    title = 'TBA';
                }

                this.$el.html(title);
                return this;
            },

            _showDetails: function () {
                var hideSeriesLink = this.column.get('hideSeriesLink');

                vent.trigger(vent.Commands.ShowEpisodeDetails, { episode: this.cellValue, hideSeriesLink: hideSeriesLink });
            }
        });
    });
