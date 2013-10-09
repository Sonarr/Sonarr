'use strict';

define(
    [
        'reqres',
        'underscore',
        'Cells/NzbDroneCell',
        'History/Queue/QueueCollection',
        'moment',
        'Shared/FormatHelpers'
    ], function (Reqres,  _, NzbDroneCell, QueueCollection, Moment, FormatHelpers) {
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
                        var episodeFile = Reqres.request(reqres.Requests.GetEpisodeFileById, this.model.get('episodeFileId'));

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

                        var downloading = _.find(QueueCollection.models, function (queueModel) {
                            return queueModel.get('episode').id === model.get('id');
                        });

                        if (downloading || this.model.get('downloading')) {
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
