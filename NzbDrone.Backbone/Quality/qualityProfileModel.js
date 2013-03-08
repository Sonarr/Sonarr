define(['app'], function () {
    NzbDrone.Quality.QualityProfileModel = Backbone.Model.extend({

        mutators: {
            allowed: function() {
                return _.where(this.get('qualities'), { allowed: true });
            },

            cutoffName: function() {
                return _.findWhere(this.get('qualities'), { id: this.get('cutoff') }).name;
            }
        },

        defaults: {
            id: null,
            name: '',
            //allowed: {},
            cutoff: null
        }
    });
});

