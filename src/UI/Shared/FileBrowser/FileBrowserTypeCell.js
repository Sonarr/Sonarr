var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'file-browser-type-cell',
    render    : function(){
        this.$el.empty();
        var type = this.model.get(this.column.get('name'));
        var icon = 'icon-hdd';
        if(type === 'computer') {
            icon = 'icon-desktop';
        }
        else if(type === 'parent') {
            icon = 'icon-level-up';
        }
        else if(type === 'folder') {
            icon = 'icon-folder-close-alt';
        }
        else if(type === 'file') {
            icon = 'icon-file-alt';
        }
        this.$el.html('<i class="{0}"></i>'.format(icon));
        this.delegateEvents();
        return this;
    }
});