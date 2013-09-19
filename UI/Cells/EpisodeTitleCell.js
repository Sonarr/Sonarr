'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell'
    ], function (App, NzbDroneCell) {
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

                App.vent.trigger(App.Commands.ShowEpisodeDetails, { episode: this.cellValue, hideSeriesLink: hideSeriesLink });
            }
        });
    });
