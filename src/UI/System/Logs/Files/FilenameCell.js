var NzbDroneCell = require('../../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'log-filename-cell',

    render : function() {
        var filename = this._getValue();
        this.$el.html(filename);

        return this;
    }
});