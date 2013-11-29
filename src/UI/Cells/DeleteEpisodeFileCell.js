'use strict';
define(
    [
        'vent',
        'backgrid'
    ], function (vent, Backgrid) {
        return Backgrid.Cell.extend({

            className : 'delete-episode-file-cell',

            events: {
                'click': '_onClick'
            },

            render: function () {
                this.$el.empty();
                this.$el.html('<i class="icon-nd-delete"></i>');

                return this;
            },

            _onClick: function () {
                var self = this;

                if (window.confirm('Are you sure you want to delete \'{0}\' form disk?'.format(this.model.get('path')))) {
                    this.model.destroy()
                        .done(function () {
                            vent.trigger(vent.Events.EpisodeFileDeleted, { episodeFile: self.model });
                        });
                }
            }
        });
    });
