'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'log-filename-cell',

            render: function () {

                var filename = this._getValue();
                this.$el.html(filename);

                return this;
            }
        });
    });
