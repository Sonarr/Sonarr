"use strict";
NzbDrone.Release.ApprovalStatusCell = Backgrid.Cell.extend({

    className: "approval-status-cell",

    render: function () {
        var rejections = this.model.get(this.column.get("name"));

        var result = '';

        _.each(rejections, function (reason) {
            result += reason + ' ';
        });

        this.$el.html(result);
        this.delegateEvents();
        return this;
    }
});
