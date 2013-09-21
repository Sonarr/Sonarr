'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell'
    ], function (App, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'series-actions-cell',

            events: {
                'click .x-edit-series'  : '_editSeries',
                'click .x-remove-series': '_removeSeries'
            },

            render: function () {
                this.$el.empty();

                this.$el.html(
                    '<i class="icon-cog x-edit-series" title="" data-original-title="Edit Series"></i> ' +
                    '<i class="icon-remove x-remove-series" title="" data-original-title="Delete Series"></i>'
                );

                this.delegateEvents();
                return this;
            },

            _editSeries: function () {
                App.vent.trigger(App.Commands.EditSeriesCommand, {series:this.model});
            },

            _removeSeries: function () {
                App.vent.trigger(App.Commands.DeleteSeriesCommand, {series:this.model});
            }
        });
    });
