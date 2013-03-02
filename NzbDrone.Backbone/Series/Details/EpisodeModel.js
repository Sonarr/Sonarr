define(['app'], function (app) {
    NzbDrone.Series.Details.EpisodeModel = Backbone.Model.extend({

        mutators: {

        },

        defaults: {
            seasonNumber: 0
        }
    });
});
