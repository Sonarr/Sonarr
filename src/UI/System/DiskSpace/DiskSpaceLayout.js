'use strict';
define([
    'vent',
    'marionette',
        'backgrid',
        'System/DiskSpace/DiskSpaceCollection',
        'Shared/LoadingView',
        'Cells/FileSizeCell'
], function (vent,Marionette,Backgrid,DiskSpaceCollection,LoadingView,FileSizeCell) {
    return Marionette.Layout.extend({
        template: 'System/DiskSpace/DiskSpaceTemplate',

        regions: {
            grid: '#x-grid'
        },
        columns:
            [
                {
                    name: 'driveLetter',
                    label: 'Drive',
                    cell: 'string'
                },
                {
                    name: 'freeSpace',
                    label: 'Free Space',
                    cell: FileSizeCell,
                    sortable:true
                },
                {
                    name: 'totalSpace',
                    label: 'Total Space',
                    cell: FileSizeCell,
                    sortable:true
                }
            ],

        initialize: function () {
            this.collection = new DiskSpaceCollection();
            this.listenTo(this.collection, 'sync', this._showTable);
        },
        onRender : function() {
            this.grid.show(new LoadingView());
        },
        
        onShow: function() {
            this.collection.fetch();
        },
        _showTable: function() {
            this.grid.show(new Backgrid.Grid({
                row: Backgrid.Row,
                columns: this.columns,
                collection: this.collection,
                className:'table table-hover'
            }));
        }
    });
});