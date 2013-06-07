"use strict";
NzbDrone.Shared.Cells.FileSizeCell = Backgrid.Cell.extend({

    className: "file-size-cell",

    render: function () {
        var size = this.model.get(this.column.get("name"));
        this.delegateEvents();
        return NzbDrone.Shared.FormatHelpers.FileSizeHelper(size);
    }
});
