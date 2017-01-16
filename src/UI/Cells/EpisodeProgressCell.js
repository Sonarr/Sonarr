var Marionette = require('marionette');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'episode-progress-cell',
    template  : 'Cells/EpisodeProgressCellTemplate',

    render : function() {

        var episodeCount = this.model.get('episodeCount');
        var episodeFileCount = this.model.get('episodeFileCount');

        var percent = 100;

        if (episodeCount > 0) {
            percent = episodeFileCount / episodeCount * 100;
        }

        this.model.set('percentOfEpisodes', percent);

        this.templateFunction = Marionette.TemplateCache.get(this.template);
        var data = this.model.toJSON();
        var html = this.templateFunction(data);
        this.$el.html(html);

        return this;
    }
});