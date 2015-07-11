var vent = require('vent');
var EpisodeTitleCell = require('../../Cells/EpisodeTitleCell');

module.exports = EpisodeTitleCell.extend({

    render : function() {
        var episode = this._getEpisode();

        if (episode) {
            EpisodeTitleCell.prototype.render.call(this);
        } else {
            this.$el.text(this.cellValue.get('title'));
        }

        return this;
    },

    _getEpisode : function() {
        var episode = this.cellValue.get('episode');

        if (episode && episode.get('id')) {
            return episode;
        } else {
            return null;
        }
    }
});