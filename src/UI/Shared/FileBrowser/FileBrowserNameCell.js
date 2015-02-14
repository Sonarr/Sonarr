var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'file-browser-name-cell',

    render : function() {
        this.$el.empty();

        var name = this.model.get(this.column.get('name'));

        this.$el.html(name);

        this.delegateEvents();

        return this;
    }
});