var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'history-quality-cell',

    render : function() {

        var title = '';
        var quality = this.model.get('quality');
        var revision = quality.revision;

        if (revision.real && revision.real > 0) {
            title += ' REAL';
        }

        if (revision.version && revision.version > 1) {
            title += ' PROPER';
        }

        title = title.trim();

        if (this.model.get('qualityCutoffNotMet')) {
            this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
        } else {
            this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
        }

        return this;
    }
});