'use strict';

define(
    [
        'backgrid'
    ], function (Backgrid) {
        return  Backgrid.Cell.extend({

            className: 'episode-status-cell',

            render: function () {
                this.$el.empty();

                if (this.model) {

                    var icon;
                    var tooltip;

                    if (this.model.get('episodeFile')) {
                        icon = 'icon-ok';
                        tooltip = 'Episode downloaded';
                    }
                    else {
                        if (this.model.get('hasAired')) {
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
