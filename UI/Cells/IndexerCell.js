"use strict";
define(['app'], function () {
    NzbDrone.Cells.IndexerCell = Backgrid.Cell.extend({

        class : 'indexer-cell',

        render: function () {
            var indexer = this.model.get(this.column.get('name'));
            this.$el.html(indexer);
            return this;
        }
    });
});
