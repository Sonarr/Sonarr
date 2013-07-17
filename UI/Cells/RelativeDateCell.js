'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment',
        'Shared/FormatHelpers'
    ], function (NzbDroneCell, Moment, FormatHelpers) {
        return NzbDroneCell.extend({

            className: 'relative-date-cell',

            render: function () {

                var date = this.model.get(this.column.get('name'));

                if (date) {
                    this.$el.html(FormatHelpers.DateHelper(date));
                    this.$el.attr('title', Moment(date).format('LLLL'));
                }

                return this;
            }
        });
    });
