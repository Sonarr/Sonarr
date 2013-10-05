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
                if (!options.episodeId) {
                    throw 'episodeId is required';
                }

                this.episodeId = options.episodeId;
            },

            fetch: function (options) {
                options = options || {};
                options.data = { episodeId: this.episodeId };

                return this.originalFetch.call(this, options);
            }
        });
    });
