'use strict';
define(
    [
        'vent',
        'backgrid'
    ], function (vent, Backgrid) {

        return Backgrid.Row.extend({
            className: 'log-file-row',

            events: {
                'click': '_showDetails'
            },

            _showDetails: function () {
                vent.trigger(vent.Commands.ShowLogFile, { model: this.model });
            }
        });
    });
