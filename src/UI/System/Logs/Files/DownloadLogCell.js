'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'download-log-cell',

            render: function () {
                this.$el.empty();
                this.$el.html('<a href="/log/{0}" class="no-router" target="_blank">Download</a>'.format(this.cellValue));

                return this;
            }
        });
    });
