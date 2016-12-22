var reqres = require('../reqres');
var vent = require('vent');
var Backbone = require('backbone');
var NzbDroneCell = require('./NzbDroneCell');
var QueueCollection = require('../Activity/Queue/QueueCollection');

module.exports = NzbDroneCell.extend({
    className : 'episode-language-cell',

    render : function() {
        this.listenTo(QueueCollection, 'sync', this._renderCell);

        this._renderCell();

        return this;
    },
    
    _renderCell : function() {
        if (this.episodeFile) {
            this.stopListening(this.episodeFile, 'change', this._refresh);
        } 
        
        this.$el.empty();

        this.episodeFile = this._getFile();
        if (this.episodeFile) {
            this.listenTo(this.episodeFile, 'change', this._refresh);
            var language = this.episodeFile.get('language');
            this.$el.html(language.name);
        } 
    },
    
    _getFile : function() {
        var hasFile = this.model.get('hasFile');

        if (hasFile) {
            var episodeFile;

            if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, this.model.get('episodeFileId'));
            }

            else if (this.model.has('episodeFile')) {
                episodeFile = new Backbone.Model(this.model.get('episodeFile'));
            }

            if (episodeFile) {
                return episodeFile;
            }
        }

        return undefined;
    }	
});