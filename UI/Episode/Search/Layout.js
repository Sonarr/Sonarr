"use strict";
define([
    'app',
    'Cells/FileSizeCell',
    'Cells/QualityCell',
    'Release/ApprovalStatusCell',
    'Release/DownloadReportCell'
], function () {

    NzbDrone.Episode.Search.Layout = Backbone.Marionette.Layout.extend({
        template: 'Episode/Search/LayoutTemplate',

        regions: {
            grid: '#episode-release-grid'
        },

        columns: [
            {
                name    : 'age',
                label   : 'Age',
                sortable: true,
                cell    : Backgrid.IntegerCell
            },
            {
                name    : 'title',
                label   : 'Title',
                sortable: true,
                cell    : Backgrid.StringCell
            },
            {
                name    : 'size',
                label   : 'Size',
                sortable: true,
                cell    : NzbDrone.Cells.FileSizeCell
            },
            {
                name    : 'quality',
                label   : 'Quality',
                sortable: true,
                cell    : NzbDrone.Cells.QualityCell
            },

            {
                name : 'rejections',
                label: 'decision',
                cell : NzbDrone.Release.ApprovalStatusCell
            },
            {
                name : 'download',
                label: '',
                cell : NzbDrone.Release.DownloadReportCell
            }
        ],

        onShow: function () {
            if (!this.isClosed) {
                this.grid.show(new Backgrid.Grid(
                    {
                        row       : Backgrid.Row,
                        columns   : this.columns,
                        collection: this.collection,
                        className : 'table table-hover'
                    }));
            }
        }
    });

});
