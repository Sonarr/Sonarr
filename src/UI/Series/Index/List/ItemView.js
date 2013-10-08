'use strict';

define(
    [
        'app',
        'marionette',
    ], function (App, Marionette) {
        return Marionette.ItemView.extend({
            template: 'Series/Index/List/ItemTemplate',

            ui: {
                'progressbar': '.progress .bar'
            },

            events: {
                'click .x-edit'  : 'editSeries',
                'click .x-remove': 'removeSeries'
            },

            editSeries: function () {
                App.vent.trigger(App.Commands.EditSeriesCommand, {series: this.model});
            },

            removeSeries: function () {
                App.vent.trigger(App.Commands.DeleteSeriesCommand, {series: this.model});
            }
        });
    });
