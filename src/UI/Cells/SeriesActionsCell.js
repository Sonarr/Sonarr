'use strict';

define(
    [
        'vent',
        'Cells/NzbDroneCell',
        'Commands/CommandController'
    ], function (vent, NzbDroneCell, CommandController) {
        return NzbDroneCell.extend({

            className: 'series-actions-cell',

            ui: {
                refresh: '.x-refresh'
            },

            events: {
                'click .x-edit'    : '_editSeries',
                'click .x-refresh' : '_refreshSeries'
            },

            render: function () {
                this.$el.empty();

                this.$el.html(
                    '<i class="icon-refresh x-refresh hidden-xs" title="" data-original-title="Update series info and scan disk"></i> ' +
                    '<i class="icon-nd-edit x-edit" title="" data-original-title="Edit Series"></i>'
                );

                CommandController.bindToCommand({
                    element: this.$el.find('.x-refresh'),
                    command: {
                        name     : 'refreshSeries',
                        seriesId : this.model.get('id')
                    }
                });

                this.delegateEvents();
                return this;
            },

            _editSeries: function () {
                vent.trigger(vent.Commands.EditSeriesCommand, {series:this.model});
            },

            _refreshSeries: function () {
                CommandController.Execute('refreshSeries', {
                    name    : 'refreshSeries',
                    seriesId: this.model.id
                });
            }
        });
    });
