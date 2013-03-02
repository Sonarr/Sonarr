define(['app', 'Series/Details/EpisodeModel'], function () {
    NzbDrone.Series.Details.EpisodeCollection = Backbone.Collection.extend({
        initialize: function(options) {
            this.seriesId = options.seriesId;
        },

        url: function(){
            return NzbDrone.Constants.ApiRoot + '/episodes/' + this.seriesId;
        },

        model: NzbDrone.Series.Details.EpisodeModel
    });
});