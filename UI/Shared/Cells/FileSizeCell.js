"use strict";
NzbDrone.Shared.Cells.FileSizeCell = Backgrid.Cell.extend({

    className: "file-size-cell",

    render: function () {

        var size = Number(this.model.get(this.column.get("name")));
        this.$el.html(size.bytes(1));
        this.delegateEvents();
        return this;

    }
});
