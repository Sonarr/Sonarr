var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'movie-monitored-cell',

    render : function() {
        this.$el.empty();
        var monitored = this.model.get('monitored');

        if (!monitored) {
            this.$el.html('<i class="icon-sonarr-unmonitored grid-icon" title="Not Monitored"></i>');
            this._setStatusWeight(2);
        }
        else {
            this.$el.html('<i class="icon-sonarr-monitored grid-icon" title="Monitored"></i>');
            this._setStatusWeight(1);
        }

        return this;
    },

    _setStatusWeight : function(weight) {
        this.model.set('statusWeight', weight, { silent : true });
    }
});