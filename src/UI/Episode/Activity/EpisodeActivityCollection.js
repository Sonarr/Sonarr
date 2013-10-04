'use strict';
define(
    [
        'backbone',
        'Episode/Activity/EpisodeActivityModel'
    ], function (Backbone, EpisodeActivityModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/episodes/activity',
            model: EpisodeActivityModel,

            originalFetch: Backbone.Collection.prototype.fetch,

            initialize: function (options) {
                this.episodeId = options.episodeId;
            },

            fetch: function (options) {
                if (!this.episodeId) {
                    throw 'episodeId is required';
                }

                if (!options) {
                    options = {};
                }

                options.data = { episodeId: this.episodeId };

                return this.originalFetch.call(this, options);
            }
        });
    });
