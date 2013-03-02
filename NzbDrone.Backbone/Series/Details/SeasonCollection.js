define(['app'], function (app) {
    NzbDrone.Series.Details.SeasonCollection = Backbone.Collection.extend({
        // Todo: Why does this throw: "this.model is undefined" - Chnaging to another model fixes it
        //model: NzbDrone.Series.Details.SeasonModel,
        model: NzbDrone.Series.Details.EpisodeModel,
        comparator: function(model) {
            return -model.get('seasonNumber');
        }
    });
});
