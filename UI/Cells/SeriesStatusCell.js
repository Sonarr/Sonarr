'use strict';
define(
    [
        'backgrid'
    ], function (Backgrid) {
        return Backgrid.Cell.extend({
            className: 'series-status-cell',

            render: function () {
                this.$el.empty();
                var monitored = this.model.get('monitored');
                var status = this.model.get('status');

                if (status === 'ended') {
                    this.$el.html('<i class="icon-stop grid-icon" title="Ended"></i>');
                    this.model.set('statusWeight', 3);
                }

                else if (!monitored) {
                    this.$el.html('<i class="icon-pause grid-icon" title="Not Monitored"></i>');
                    this.model.set('statusWeight', 2);
                }

                else {
                    this.$el.html('<i class="icon-play grid-icon" title="Continuing"></i>');
                    this.model.set('statusWeight', 1);
                }

                return this;
            }
        });
    });
