var NzbDroneCell = require('../../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'download-log-cell',

    render : function() {
        this.$el.empty();
        this.$el.html('<a href="{0}" class="no-router" target="_blank">Download</a>'.format(this.cellValue));

        return this;
    }
});