var _ = require('underscore');
var vent = require('../../vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var SelectEpisodeLayout = require('../Episode/SelectEpisodeLayout');

module.exports = NzbDroneCell.extend({
    className : 'episodes-cell',

    events : {
        'click' : '_onClick'
    },

    render : function() {
        this.$el.empty();

        var episodes = this.model.get('episodes');

        if (episodes)
        {
            var episodeNumbers = _.map(episodes, 'episodeNumber');

            this.$el.html(episodeNumbers.join(', '));
        }

        return this;
    },

    _onClick : function () {
        var series = this.model.get('series');
        var seasonNumber = this.model.get('seasonNumber');

        if (series === undefined || seasonNumber === undefined) {
            return;
        }

        var view =  new SelectEpisodeLayout({ series: series, seasonNumber: seasonNumber });

        this.listenTo(view, 'manualimport:selected:episodes', this._setEpisodes);

        vent.trigger(vent.Commands.OpenModal2Command, view);
    },

    _setEpisodes : function (e) {
        this.model.set('episodes', e.episodes);
    }
});