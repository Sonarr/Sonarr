'use strict';

define(
    [
        'backgrid',
        'moment'
    ], function (Backgrid, Moment) {
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
                        var quality = this.model.get('episodeFile').quality;

                        if (quality.proper) {
                            this.$el.html('<span class="badge badge-info" title="Episode downloaded [PROPER]">{0}</span>'.format(quality.quality.name));
                        }

                        else {
                            this.$el.html('<span class="badge badge-inverse" title="Episode downloaded">{0}</span>'.format(quality.quality.name));
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
