'use strict';
define(
    [
        'backbone',
        'Series/EpisodeFileModel'
    ], function (Backbone, EpisodeFileModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/episodefile',
            model: EpisodeFileModel,

            originalFetch: Backbone.Collection.prototype.fetch,

            initialize: function (options) {
                this.seriesId = options.seriesId;
            },

            fetch: function (options) {
                if (!this.seriesId) {
                    throw 'seriesId is required';
                }

                if (!options) {
                    options = {};
                }

                options['data'] = { seriesId: this.seriesId };

                return this.originalFetch.call(this, options);
            }
        });
    });
