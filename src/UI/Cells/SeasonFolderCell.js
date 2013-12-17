'use strict';
define(
    [
        'backgrid'
    ], function (Backgrid) {
        return Backgrid.Cell.extend({

            className : 'season-folder-cell',

            render: function () {
                var seasonFolder = this.model.get('seasonFolder');
                this.$el.html(seasonFolder.toString());
                return this;
            }
        });
    });
