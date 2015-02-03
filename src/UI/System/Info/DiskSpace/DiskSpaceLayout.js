var vent = require('../../../vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var DiskSpaceCollection = require('./DiskSpaceCollection');
var LoadingView = require('../../../Shared/LoadingView');
var DiskSpacePathCell = require('./DiskSpacePathCell');
var FileSizeCell = require('../../../Cells/FileSizeCell');

module.exports = Marionette.Layout.extend({
    template   : 'System/Info/DiskSpace/DiskSpaceLayoutTemplate',
    regions    : {grid : '#x-grid'},
    columns    : [{
        name     : 'path',
        label    : 'Location',
        cell     : DiskSpacePathCell,
        sortable : false
    }, {
        name     : 'freeSpace',
        label    : 'Free Space',
        cell     : FileSizeCell,
        sortable : false
    }, {
        name     : 'totalSpace',
        label    : 'Total Space',
        cell     : FileSizeCell,
        sortable : false
    }],
    initialize : function(){
        this.collection = new DiskSpaceCollection();
        this.listenTo(this.collection, 'sync', this._showTable);
    },
    onRender   : function(){
        this.grid.show(new LoadingView());
    },
    onShow     : function(){
        this.collection.fetch();
    },
    _showTable : function(){
        this.grid.show(new Backgrid.Grid({
            row        : Backgrid.Row,
            columns    : this.columns,
            collection : this.collection,
            className  : 'table table-hover'
        }));
    }
});