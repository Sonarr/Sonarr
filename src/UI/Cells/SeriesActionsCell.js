'use strict';

define(
    [
        'vent',
        'Cells/NzbDroneCell'
    ], function (vent, NzbDroneCell) {
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
                vent.trigger(vent.Commands.EditSeriesCommand, {series:this.model});
            },

            _removeSeries: function () {
                vent.trigger(vent.Commands.DeleteSeriesCommand, {series:this.model});
            }
        });
    });
