define(['app'], function () {
    NzbDrone.Series.EpisodeModel = Backbone.Model.extend({

        mutators: {

        },

        defaults: {
            seasonNumber: 0
        }
    });
});
