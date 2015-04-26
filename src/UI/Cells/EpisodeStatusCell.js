var reqres = require('../reqres');
var Backbone = require('backbone');
var NzbDroneCell = require('./NzbDroneCell');
var QueueCollection = require('../Activity/Queue/QueueCollection');
var moment = require('moment');
var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'episode-status-cell',

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

        if (this.model) {

            var icon;
            var tooltip;

            var hasAired = moment(this.model.get('airDateUtc')).isBefore(moment());
            this.episodeFile = this._getFile();

            if (this.episodeFile) {
                this.listenTo(this.episodeFile, 'change', this._refresh);

                var quality = this.episodeFile.get('quality');
                var revision = quality.revision;
                var size = FormatHelpers.bytes(this.episodeFile.get('size'));
                var title = 'Episode downloaded';

                if (revision.real && revision.real > 0) {
                    title += '[REAL]';
                }

                if (revision.version && revision.version > 1) {
                    title += ' [PROPER]';
                }

                if (size !== '') {
                    title += ' - {0}'.format(size);
                }

                if (this.episodeFile.get('qualityCutoffNotMet')) {
                    this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
                } else {
                    this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
                }

                return;
            }

            else {
                var model = this.model;
                var downloading = QueueCollection.findEpisode(model.get('id'));

                if (downloading) {
                    var progress = 100 - (downloading.get('sizeleft') / downloading.get('size') * 100);

                    if (progress === 0) {
                        icon = 'icon-sonarr-downloading';
                        tooltip = 'Episode is downloading';
                    }

                    else {
                        this.$el.html('<div class="progress" title="Episode is downloading - {0}% {1}">'.format(progress.toFixed(1), downloading.get('title')) +
                                      '<div class="progress-bar progress-bar-purple" style="width: {0}%;"></div></div>'.format(progress));
                        return;
                    }
                }

                else if (this.model.get('grabbed')) {
                    icon = 'icon-sonarr-downloading';
                    tooltip = 'Episode is downloading';
                }

                else if (!this.model.get('airDateUtc')) {
                    icon = 'icon-sonarr-tba';
                    tooltip = 'TBA';
                }

                else if (hasAired) {
                    icon = 'icon-sonarr-missing';
                    tooltip = 'Episode missing from disk';
                } else {
                    icon = 'icon-sonarr-not-aired';
                    tooltip = 'Episode has not aired';
                }
            }

            this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, tooltip));
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