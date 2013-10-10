'use strict';
define([
    'vent',
    'marionette',
        'backgrid',
        'System/DiskSpace/DiskSpaceCollection',
        'Shared/LoadingView'
], function (vent,Marionette,Backgrid,DiskSpaceCollection,LoadingView) {
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
                    cell: Backgrid.StringCell
                },
                {
                    name: 'freeSpace',
                    label: 'Free Space',
                    cell: Backgrid.StringCell
                },
                {
                    name: 'totalSpace',
                    label: 'Total Space',
                    cell: Backgrid.StringCell
                }
            ],

        initialize: function () {
            this.collection = new DiskSpaceCollection();
            this.collectionPromise = this.collection.fetch();

            vent.on(vent.Events.CommandComplete, this._commandComplete, this);
        },
        onRender : function() {
            this.grid.show(new LoadingView());
        },
        
        onShow: function() {
            var self = this;
            this.collectionPromise.done(function() {
                self._showTable();
            });
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