'use strict';

define(
    [
        'backgrid',
        'Shared/FormatHelpers'
    ], function (Backgrid, FormatHelpers) {
        return Backgrid.Cell.extend({

            className: 'file-size-cell',

            render: function () {
                var size = this.model.get(this.column.get('name'));
                this.$el.html(FormatHelpers.bytes(size));
                this.delegateEvents();
                return this;
            }
        });
    });
