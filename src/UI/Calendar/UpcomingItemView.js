'use strict';

define(
    [
        'vent',
        'marionette',
        'moment'
    ], function (vent, Marionette, Moment) {
        return Marionette.ItemView.extend({
            template: 'Calendar/UpcomingItemViewTemplate',
            tagName : 'div',

            events: {
                'click .x-episode-title': '_showEpisodeDetails'
            },

            initialize: function () {
                var start = this.model.get('airDateUtc');
                var runtime = this.model.get('series').runtime;
                var end = Moment(start).add('minutes', runtime);

                this.model.set({
                    end: end.toISOString()
                });
            },

            _showEpisodeDetails: function () {
                vent.trigger(vent.Commands.ShowEpisodeDetails, {episode: this.model});
            }
        });
    });
