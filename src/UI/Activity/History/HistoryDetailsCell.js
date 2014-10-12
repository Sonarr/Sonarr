'use strict';

define(
    [
        'vent',
        'Cells/NzbDroneCell'
    ], function (vent, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'history-details-cell',

            events: {
                'click': '_showDetails'
            },

            render: function () {
                this.$el.empty();
                this.$el.html('<i class="icon-info-sign"></i>');

                return this;
            },

            _showDetails: function () {
                vent.trigger(vent.Commands.ShowHistoryDetails, { model: this.model });
            }
        });
    });
