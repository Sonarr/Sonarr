'use strict';
define(
    [
        'underscore',
        'vent',
        'backbone',
        'marionette',
        'Commands/CommandController'
    ], function (_, vent, Backbone, Marionette, CommandController) {

        return  Marionette.ItemView.extend({
            template: 'Series/Editor/Rename/RenameSeriesViewTemplate',

            events: {
                'click .x-confirm-rename': '_rename'
            },

            initialize: function (options) {
                this.series = options.series;

                this.templateHelpers = { numberOfSeries: this.series.length, series: new Backbone.Collection(this.series).toJSON() };
            },

            _rename: function () {
                var seriesIds = _.pluck(this.series, 'id');

                CommandController.Execute('renameSeries', {
                    name      : 'renameSeries',
                    seriesIds : seriesIds
                });

                this.trigger('seriesRenamed');
                vent.trigger(vent.Commands.CloseModalCommand);
            }
        });
    });
