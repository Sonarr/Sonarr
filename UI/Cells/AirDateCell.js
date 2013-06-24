'use strict';
define(
    [
        'backgrid',
        'Shared/FormatHelpers'
    ], function (Backgrid, FormatHelpers) {
        return Backgrid.Cell.extend({
            className: 'air-date-cell',

            render: function () {

                this.$el.empty();
                var airDate = this.model.get(this.column.get('name'));
                this.$el.html(FormatHelpers.DateHelper(airDate));
                return this;

            }
        });
    });
