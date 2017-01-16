var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'release-title-cell',

    render : function() {
        this.$el.empty();

        var title = this.model.get('title');
        var infoUrl = this.model.get('infoUrl');

        if (infoUrl) {
            this.$el.html('<a href="{0}">{1}</a>'.format(infoUrl, title));
        } else {
            this.$el.html(title);
        }

        return this;
    }
});