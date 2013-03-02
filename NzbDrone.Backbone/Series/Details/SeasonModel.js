define(['app', 'Series/Details/SeasonCollection'], function (app) {
    NzbDrone.Series.Details.SeasonModel = Backbone.Model.extend({
        //Season Number
        //Episodes

        initialize: function(options) {
            this.seasonNumber = options.seasonNumber;
            this.episodes = options.episodes;
        }
    });
});