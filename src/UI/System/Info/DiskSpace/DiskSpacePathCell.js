var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'disk-space-path-cell',

    render : function() {
        this.$el.empty();

        var path = this.model.get('path');
        var label = this.model.get('label');

        var contents = path;

        if (label) {
            contents += ' ({0})'.format(label);
        }

        this.$el.html(contents);

        return this;
    }
});