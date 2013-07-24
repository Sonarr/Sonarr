'use strict';
define(
    [
        'app',
        'backgrid'
    ], function (App, Backgrid) {
        return Backgrid.Row.extend({
            events: {
                'click .x-edit'  : 'editSeries',
                'click .x-remove': 'removeSeries'
            },

            editSeries: function () {
                App.vent.trigger(App.Commands.EditSeriesCommand, {series:this.model});
            },

            removeSeries: function () {
                App.vent.trigger(App.Commands.DeleteSeriesCommand, {series:this.model});
            },
        });
    });

