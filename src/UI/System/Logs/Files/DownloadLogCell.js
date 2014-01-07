'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'System/StatusModel'
    ], function (NzbDroneCell, StatusModel) {
        return NzbDroneCell.extend({

            className: 'download-log-cell',

            render: function () {
                this.$el.empty();
                this.$el.html('<a href="{0}/log/{1}" class="no-router" target="_blank">Download</a>'.format(StatusModel.get('urlBase'), this.cellValue));

                return this;
            }
        });
    });
