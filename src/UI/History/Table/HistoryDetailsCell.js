'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell'
    ], function (App, NzbDroneCell) {
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
                App.vent.trigger(App.Commands.ShowHistoryDetails, { history: this.model });
            }
        });
    });
