'use strict';

define(
    [
        'vent',
        'marionette',
        'moment'
    ], function (vent, Marionette, moment) {
        return Marionette.ItemView.extend({
            template: 'Calendar/UpcomingItemViewTemplate',
            tagName : 'div',

            events: {
                'click .x-episode-title': '_showEpisodeDetails'
            },

            initialize: function () {
                var start = this.model.get('airDateUtc');
                var runtime = this.model.get('series').runtime;
                var end = moment(start).add('minutes', runtime);

                this.model.set({
                    end: end.toISOString()
                });

                this.listenTo(this.model, 'change', this.render);
            },

            _showEpisodeDetails: function () {
                vent.trigger(vent.Commands.ShowEpisodeDetails, {episode: this.model});
            }
        });
    });
