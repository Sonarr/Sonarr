var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'season-folder-cell',

    render : function() {
        this.$el.empty();

        var seasonFolder = this.model.get(this.column.get('name'));
        this.$el.html(seasonFolder.toString());

        return this;
    }
});