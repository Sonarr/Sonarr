var reqres = require('../reqres');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'episode-file-path-cell',

    render : function() {
        this.$el.empty();
        this.$el.html(this.model.get('relativePath'));
        this.delegateEvents();
        return this;
    }
});