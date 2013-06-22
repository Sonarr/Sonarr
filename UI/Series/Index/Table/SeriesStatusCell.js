'use strict';
define(['app','backgrid'], function () {
    Backgrid.SeriesStatusCell = Backgrid.Cell.extend({
        className: 'series-status-cell',

        render: function () {
            this.$el.empty();
            var monitored = this.model.get('monitored');
            var status = this.model.get('status');

            if (!monitored) {
                this.$el.html('<i class='icon-pause grid-icon' title='Not Monitored'></i>');
            }
            else if (status === 'continuing') {
                this.$el.html('<i class='icon-play grid-icon' title='Continuing'></i>');
            }

            else {
                this.$el.html('<i class='icon-stop grid-icon' title='Ended'></i>');
            }

            return this;
        }
    });

    return Backgrid.SeriesStatusCell;
});
