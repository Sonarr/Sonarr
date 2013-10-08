'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Cells/ApprovalStatusCell',
        'Release/DownloadReportCell',
        'Cells/Header/QualityHeaderCell'

    ], function (Marionette, Backgrid, FileSizeCell, QualityCell, ApprovalStatusCell, DownloadReportCell, QualityHeaderCell) {

        return Marionette.Layout.extend({
            template: 'Episode/Search/ManualLayoutTemplate',

            regions: {
                grid: '#episode-release-grid'
            },

            columns:
                [
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
                        cell    : Backgrid.StringCell.extend({ className: 'nzb-title-cell' })
                    },
                    {
                        name    : 'indexer',
                        label   : 'Indexer',
                        sortable: true,
                        cell    : Backgrid.StringCell
                    },
                    {
                        name    : 'size',
                        label   : 'Size',
                        sortable: true,
                        cell    : FileSizeCell
                    },
                    {
                        name      : 'quality',
                        label     : 'Quality',
                        sortable  : true,
                        cell      : QualityCell,
                        headerCell: QualityHeaderCell
                    },

                    {
                        name : 'rejections',
                        label: '',
                        cell : ApprovalStatusCell
                    },
                    {
                        name : 'download',
                        label: '',
                        cell : DownloadReportCell
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
