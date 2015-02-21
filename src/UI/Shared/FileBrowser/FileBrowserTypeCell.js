var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'file-browser-type-cell',

    render : function() {
        this.$el.empty();

        var type = this.model.get(this.column.get('name'));
        var icon = 'icon-sonarr-hdd';

        if (type === 'computer') {
            icon = 'icon-sonarr-browser-computer';
        } else if (type === 'parent') {
            icon = 'icon-sonarr-browser-up';
        } else if (type === 'folder') {
            icon = 'icon-sonarr-browser-folder';
        } else if (type === 'file') {
            icon = 'icon-sonarr-browser-file';
        }

        this.$el.html('<i class="{0}"></i>'.format(icon));
        this.delegateEvents();

        return this;
    }
});