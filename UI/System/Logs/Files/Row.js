'use strict';
define(
    [
        'app',
        'backgrid'
    ], function (App, Backgrid) {

        return Backgrid.Row.extend({
            className: 'log-file-row',

            events: {
                'click': '_showContents'
            },

            _showContents: function () {
                App.vent.trigger(App.Commands.ShowLogFile, { model: this.model });
            }
        });
    });
