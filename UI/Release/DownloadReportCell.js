"use strict";

define(['app','backgrid'], function () {
    NzbDrone.Release.DownloadReportCell = Backgrid.Cell.extend({

        className: "download-report-cell",

        events: {
            'click': '_onClick'
        },

        _onClick: function () {

            var self = this;

            this.$el.html('<i class ="icon-spinner icon-spin" />');
            this.model.save()
                .always(function () {
                    self.$el.html('<i class ="icon-download-alt" />');
                });
        },

        render: function () {

            this.$el.html('<i class ="icon-download-alt" />');
            return this;

        }
    });


    return NzbDrone.Release.DownloadReportCell;
});
