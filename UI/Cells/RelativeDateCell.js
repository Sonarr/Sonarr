"use strict";
define(['app'], function () {
    NzbDrone.Cells.RelativeDateCell = Backgrid.Cell.extend({

        render: function () {

            var date = this.model.get(this.column.get('name'));
            this.$el.html(Date.create(date).relative());

            return this;
        }
    });
});
