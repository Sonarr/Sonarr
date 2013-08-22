'use strict';

define(
    [
        'app',
        'Cells/NzbDroneCell',
        'moment',
        'Shared/FormatHelpers'
    ], function (App, NzbDroneCell, Moment, FormatHelpers) {
        return  NzbDroneCell.extend({

            className: 'episode-status-cell',

            render: function () {
                this.$el.empty();

                if (this.model) {

                    var icon;
                    var tooltip;

                    var hasAired = Moment(this.model.get('airDateUtc')).isBefore(Moment());
                    var hasFile = this.model.get('hasFile');

                    if (hasFile) {
                        var episodeFile = App.request(App.Reqres.GetEpisodeFileById, this.model.get('episodeFileId'));

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

                        return this;
                    }
                    else {
                        if (this.model.get('downloading')) {
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

                return this;
            }
        });
    });
