'use strict';

define(
    [
        'backgrid',
        'moment',
        'Shared/FormatHelpers'
    ], function (Backgrid, Moment, FormatHelpers) {
        return  Backgrid.Cell.extend({

            className: 'episode-status-cell',

            render: function () {
                this.$el.empty();

                if (this.model) {

                    var icon;
                    var tooltip;

                    var hasAired = Moment(this.model.get('airDateUtc')).isBefore(Moment());
                    var hasFile = this.model.get('hasFile');

                    if (hasFile) {
                        var episodeFile = this.model.get('episodeFile');
                        var quality = episodeFile.quality;
                        var size = FormatHelpers.bytes(episodeFile.size);
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
                        if (!this.model.get('airDateUtc')) {
                            icon = 'icon-question-sign';
                            tooltip = 'TBA';
                        }

                        else if (hasAired) {
                            icon = 'icon-warning-sign';
                            tooltip = 'Episode missing from disk';
                        }
                        else {
                            icon = 'icon-time';
                            tooltip = 'Episode has not aired';
                        }
                    }

                    this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, tooltip));
                }

                return this;
            }
        });
    });
