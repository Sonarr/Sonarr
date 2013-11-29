'use strict';
define(
    [
        'backgrid'
    ], function (Backgrid) {
        return Backgrid.Cell.extend({

            className : 'indexer-cell',

            render: function () {
                var indexer = this.model.get(this.column.get('name'));
                this.$el.html(indexer);
                return this;
            }
        });
    });
