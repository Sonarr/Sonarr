'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'log-level-cell',

            render: function () {

                var level = this._getValue();
                this.$el.html('<i class="icon-nd-health-{0}" title="{1}"/>'.format(this._getValue().toLowerCase(), level));

                return this;
            }
        });
    });
