"use strict";
define(['app', 'Shared/Cells/FileSizeCell'], function () {

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
                name    : 'size',
                label   : 'Size',
                sortable: true,
                cell    : NzbDrone.Shared.Cells.FileSizeCell
            },
            {
                name    : 'title',
                label   : 'Title',
                sortable: true,
                cell    : Backgrid.StringCell
            },
            {
                name : 'approved',
                label: 'Approved',
                cell : Backgrid.BooleanCell
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
