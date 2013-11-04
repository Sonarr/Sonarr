'use strict';
define(
    [
        'vent',
        'backgrid'
    ], function (vent, Backgrid) {

        return Backgrid.Row.extend({
            className: 'log-row',

            events: {
                'click': '_showDetails'
            },

            _showDetails: function () {
                vent.trigger(vent.Commands.ShowLogDetails, { model: this.model });
            }
        });
    });
