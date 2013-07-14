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
                this.$el.html(date.format('LT'));
                this.$el.attr('title', date.format('LLLL'));

                return this;
            }
        });
    });
