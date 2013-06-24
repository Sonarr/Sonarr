'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'relative-date-cell',

            render: function () {

                var date = this.model.get(this.column.get('name'));
                this.$el.html(Date.create(date).relative());

                return this;
            }
        });
    });
