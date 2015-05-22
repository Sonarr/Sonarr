var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'path-cell',

    render : function() {
        this.$el.empty();

        var relativePath = this.model.get('relativePath');
        var path = this.model.get('path');

        this.$el.html('<div title="{0}">{1}</div>'.format(path, relativePath));

        return this;
    }
});