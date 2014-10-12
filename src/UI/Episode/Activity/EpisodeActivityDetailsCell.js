'use strict';

define(
    [
        'jquery',
        'vent',
        'marionette',
        'Cells/NzbDroneCell',
        'Activity/History/Details/HistoryDetailsView',
        'bootstrap'
    ], function ($, vent, Marionette, NzbDroneCell, HistoryDetailsView) {
        return NzbDroneCell.extend({

            className: 'episode-activity-details-cell',


            render: function () {
                this.$el.empty();
                this.$el.html('<i class="icon-info-sign"></i>');

                var html = new HistoryDetailsView({ model: this.model }).render().$el;

                this.$el.popover({
                    content  : html,
                    html     : true,
                    trigger  : 'hover',
                    title    : 'Details',
                    placement: 'left',
                    container: this.$el
                });

                return this;
            }
        });
    });
