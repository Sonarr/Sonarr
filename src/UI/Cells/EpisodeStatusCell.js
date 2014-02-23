'use strict';

define(
    [
        'reqres',
        'backbone',
        'Cells/NzbDroneCell',
        'History/Queue/QueueCollection',
        'moment',
        'Shared/FormatHelpers'
    ], function (reqres, Backbone, NzbDroneCell, QueueCollection, Moment, FormatHelpers) {
        return  NzbDroneCell.extend({

            className: 'episode-status-cell',

            render: function () {
                this.listenTo(QueueCollection, 'sync', this._renderCell);

                this._renderCell();

                return this;
            },

            _renderCell: function () {
                this.$el.empty();

                if (this.model) {

                    var icon;
                    var tooltip;

                    var hasAired = Moment(this.model.get('airDateUtc')).isBefore(Moment());
                    var hasFile = this.model.get('hasFile');

                    if (hasFile) {
                        var episodeFile;

                        if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                            episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, this.model.get('episodeFileId'));
                        }

                        else {
                            episodeFile = new Backbone.Model(this.model.get('episodeFile'));
                        }

                        this.listenTo(episodeFile, 'change', this._refresh);

                        var quality = episodeFile.get('quality');
                        var size = FormatHelpers.bytes(episodeFile.get('size'));
                        var title = 'Episode downloaded';

                        if (quality.proper) {
                            title += ' [PROPER] - {0}'.format(size);
                            this.$el.html('<span class="badge badge-info" title="{0}">{1}</span>'.format(title, quality.quality.name));
                        }

                        else {
                            title += ' - {0}'.format(size);
                            this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
                        }

                        return;
                    }

                    else {
                        var model = this.model;
                        var downloading = QueueCollection.findEpisode(model.get('id'));

                        if (downloading) {
                            var progress = 100 - (downloading.get('sizeleft') / downloading.get('size') * 100);

                            this.$el.html('<div class="progress progress-purple" title="Episode is downloading - {0}%" data-container="body">'.format(progress.toFixed(1)) +
                                          '<div class="bar" style="width: {0}%;"></div></div>'.format(progress));
                            return;
                        }

                        else if (this.model.get('downloading')) {
                            icon = 'icon-nd-downloading';
                            tooltip = 'Episode is downloading';
                        }

                        else if (!this.model.get('airDateUtc')) {
                            icon = 'icon-nd-tba';
                            tooltip = 'TBA';
                        }

                        else if (hasAired) {
                            icon = 'icon-nd-missing';
                            tooltip = 'Episode missing from disk';
                        }
                        else {
                            icon = 'icon-nd-not-aired';
                            tooltip = 'Episode has not aired';
                        }
                    }

                    this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, tooltip));
                }
            }
        });
    });
