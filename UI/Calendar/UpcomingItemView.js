'use strict';

define(
    [
        'app',
        'marionette'
    ], function (App, Marionette) {
        return Marionette.ItemView.extend({
            template: 'Calendar/UpcomingItemTemplate',
            tagName : 'div',

            events: {
                'click .x-episode-title': '_showEpisodeDetails'
            },

            _showEpisodeDetails: function () {
                App.vent.trigger(App.Commands.ShowEpisodeDetails, {episode: this.model});
            }
        });
    });
