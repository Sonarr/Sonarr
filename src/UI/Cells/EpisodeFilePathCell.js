var reqres = require('../reqres');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'episode-file-path-cell',

    render : function() {
        this.$el.empty();

        if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
            var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, this.model.get('episodeFileId'));

            this.$el.html(episodeFile.get('relativePath'));
        }

        this.delegateEvents();
        return this;
    }
});