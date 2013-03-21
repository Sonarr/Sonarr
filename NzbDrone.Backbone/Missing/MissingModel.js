define(['app'], function (app) {
    NzbDrone.Missing.MissingModel = Backbone.Model.extend({
        mutators: {
            bestDateString: function () {
                return bestDateString(this.get('airDate'));
            },
            paddedEpisodeNumber: function(){
                return this.get('episodeNumber');
            }
        }
    });
});
