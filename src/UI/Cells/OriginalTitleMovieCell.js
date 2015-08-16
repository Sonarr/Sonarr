var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');
var MovieFileCollection = require('../Movie/MovieFileCollection');

module.exports = NzbDroneCell.extend({
    className : 'episode-title-cell',

    events : {
        'click' : '_showDetails'
    },

    render : function() {
        var title = this.cellValue.get('originalTitle');

        if (!title || title === '') {
            title = this.cellValue.get('title');
        }

        this.$el.html(title);
        return this;
    },

    _showDetails : function() {
        var hideMovieLink = this.column.get('hideSeriesLink');
        vent.trigger(vent.Commands.ShowMovieDetails, {
            movie               : this.cellValue,
            hideMovieLink       : hideMovieLink,
            movieFileCollection : new MovieFileCollection({ movieId : this.cellValue.id })
        });
    }
});