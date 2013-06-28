'use strict';

define(
    [
        'app',
        'marionette',
        'Episode/Layout'
    ], function (App, Marionette, EpisodeLayout) {
        return Marionette.ItemView.extend({
            template: 'Calendar/UpcomingItemTemplate',
            tagName : 'div',

            events  : {
                'click .x-episode-title' : '_showEpisodeDetails'
            },

            _showEpisodeDetails : function() {
                var view = new EpisodeLayout({ model: this.model });
                App.modalRegion.show(view);
            }
        });
    });
