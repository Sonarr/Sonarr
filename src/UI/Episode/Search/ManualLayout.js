var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ReleaseTitleCell = require('../../Cells/ReleaseTitleCell');
var FileSizeCell = require('../../Cells/FileSizeCell');
var QualityCell = require('../../Cells/QualityCell');
var ApprovalStatusCell = require('../../Cells/ApprovalStatusCell');
var DownloadReportCell = require('../../Release/DownloadReportCell');
var AgeCell = require('../../Release/AgeCell');
var ProtocolCell = require('../../Release/ProtocolCell');
var PeersCell = require('../../Release/PeersCell');

module.exports = Marionette.Layout.extend({
    template : 'Episode/Search/ManualLayoutTemplate',

    regions : {
        grid : '#episode-release-grid'
    },

    columns : [
        {
            name  : 'protocol',
            label : 'Source',
            cell  : ProtocolCell
        }, {
            name  : 'age',
            label : 'Age',
            cell  : AgeCell
        }, {
            name  : 'title',
            label : 'Title',
            cell  : ReleaseTitleCell
        }, {
            name  : 'indexer',
            label : 'Indexer',
            cell  : Backgrid.StringCell
        }, {
            name  : 'size',
            label : 'Size',
            cell  : FileSizeCell
        }, {
            name  : 'seeders',
            label : 'Peers',
            cell  : PeersCell
        }, {
            name  : 'quality',
            label : 'Quality',
            cell  : QualityCell
        }, {
            name     : 'rejections',
            label    : '',
            cell     : ApprovalStatusCell,
            sortable : false
        }, {
            name     : 'download',
            label    : '',
            cell     : DownloadReportCell,
            sortable : true
        }
    ],

    onShow : function() {
        if (!this.isClosed) {
            this.grid.show(new Backgrid.Grid({
                row        : Backgrid.Row,
                columns    : this.columns,
                collection : this.collection,
                className  : 'table table-hover'
            }));
        }
    }
});