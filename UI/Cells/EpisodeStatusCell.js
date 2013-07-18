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

                    var hasAired = Moment(this.model.get('airDate')).isBefore(Moment());
                    var hasFile = this.model.get('hasFile');

                    if (hasFile) {
                        icon = 'icon-ok';
                        tooltip = 'Episode downloaded';
                    }
                    else {
                        if (!this.model.get('airDate')) {
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
