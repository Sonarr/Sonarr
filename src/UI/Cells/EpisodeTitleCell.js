var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'episode-title-cell',

    events : {
        'click' : '_showDetails'
    },

    render : function() {
        var title = this.cellValue.get('title');

        if (!title || title === '') {
            title = 'TBA';
        }

        this.$el.text(title);
        return this;
    },

    _refresh : function() {
        this.$el.toggleClass('clickable', this._getEpisode());

        NzbDroneCell.prototype._refresh.call(this);
    },

    _getEpisode : function() {
        var episode = this.cellValue;

        if (episode && episode.get('id')) {
            return episode;
        } else {
            return null;
        }
    },

    _showDetails : function() {
        var episode = this._getEpisode();

        if (!episode) {
            return;
        }

        var hideSeriesLink = this.column.get('hideSeriesLink');
        vent.trigger(vent.Commands.ShowEpisodeDetails, {
            episode        : episode,
            hideSeriesLink : hideSeriesLink
        });
    }
});