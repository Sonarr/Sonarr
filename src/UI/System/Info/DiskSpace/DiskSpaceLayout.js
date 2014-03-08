'use strict';
define([
        'vent',
        'marionette',
        'backgrid',
        'System/Info/DiskSpace/DiskSpaceCollection',
        'Shared/LoadingView',
        'System/Info/DiskSpace/DiskSpacePathCell',
        'Cells/FileSizeCell'
], function (vent,Marionette,Backgrid,DiskSpaceCollection,LoadingView, DiskSpacePathCell, FileSizeCell) {
    return Marionette.Layout.extend({
        template: 'System/Info/DiskSpace/DiskSpaceLayoutTemplate',

        regions: {
            grid: '#x-grid'
        },

        columns:
            [
                {
                    name: 'path',
                    label: 'Location',
                    cell: DiskSpacePathCell
                },
                {
                    name: 'freeSpace',
                    label: 'Free Space',
                    cell: FileSizeCell
                },
                {
                    name: 'totalSpace',
                    label: 'Total Space',
                    cell: FileSizeCell
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