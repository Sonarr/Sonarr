'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'queue-status-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {
                    var status = this.cellValue.get('status').toLowerCase();
                    var icon = 'icon-nd-downloading';
                    var title = 'Downloading';

                    if (status === 'paused') {
                        icon = 'icon-pause';
                        title = 'Paused';
                    }

                    if (status === 'queued') {
                        icon = 'icon-cloud';
                        title = 'Queued';
                    }

                    var timeleft = this.cellValue.get('timeleft');
                    var size = this.cellValue.get('size');
                    var sizeleft = this.cellValue.get('sizeleft');

                    this.$el.html('<i class="{0}" title="{1}"></i>'.format(icon, title));
                }

                return this;
            }
        });
    });
