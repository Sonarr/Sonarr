'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment'
    ], function (NzbDroneCell, Moment) {
        return NzbDroneCell.extend({

            className: 'log-time-cell',

            render: function () {

                var date = Moment(this._getValue());
                this.$el.html('<span title="{1}">{0}</span>'.format(date.format('LT'), date.format('LLLL')));

                return this;
            }
        });
    });
