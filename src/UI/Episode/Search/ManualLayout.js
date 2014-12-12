'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Cells/ReleaseTitleCell',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Cells/ApprovalStatusCell',
        'Release/DownloadReportCell',
        'Release/AgeCell'
    ], function (Marionette, Backgrid, ReleaseTitleCell, FileSizeCell, QualityCell, ApprovalStatusCell, DownloadReportCell, AgeCell) {

        return Marionette.Layout.extend({
            template: 'Episode/Search/ManualLayoutTemplate',

            regions: {
                grid: '#episode-release-grid'
            },

            columns:
                [
                    {
                        name     : 'age',
                        label    : 'Age',
                        cell     : AgeCell
                    },
                    {
                        name     : 'title',
                        label    : 'Title',
                        cell     : ReleaseTitleCell
                    },
                    {
                        name     : 'indexer',
                        label    : 'Indexer',
                        cell     : Backgrid.StringCell
                    },
                    {
                        name     : 'size',
                        label    : 'Size',
                        cell     : FileSizeCell
                    },
                    {
                        name     : 'quality',
                        label    : 'Quality',
                        cell     : QualityCell
                    },
                    {
                        name     : 'rejections',
                        label    : '',
                        cell     : ApprovalStatusCell,
                        sortable : false
                    },
                    {
                        name     : 'download',
                        label    : '',
                        cell     : DownloadReportCell,
                        sortable : false
                    }
                ],

            onShow: function () {
                if (!this.isClosed) {
                    this.grid.show(new Backgrid.Grid({
                        row       : Backgrid.Row,
                        columns   : this.columns,
                        collection: this.collection,
                        className : 'table table-hover'
                    }));
                }
            }
        });

    });
