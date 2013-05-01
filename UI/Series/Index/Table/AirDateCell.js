"use strict";
Backgrid.AirDateCell = Backgrid.Cell.extend({
    className: "air-date-cell",

    render: function () {
        this.$el.empty();
        var airDate = this.model.get(this.column.get("name"));

        this.$el.html(bestDateString(airDate));

        return this;
    }
});
