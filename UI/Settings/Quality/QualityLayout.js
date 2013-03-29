define([
    'app',
    'Quality/QualityProfileCollection',
    'Quality/QualitySizeCollection',
    'Settings/Quality/QualityView',
    'Settings/Quality/Profile/QualityProfileCollectionView',
    'Settings/Quality/Size/QualitySizeCollectionView'
],
    function (app, qualityProfileCollection) {
        NzbDrone.Settings.Quality.QualityLayout = Backbone.Marionette.Layout.extend({
            template: 'Settings/Quality/QualityLayoutTemplate',

            regions: {
                qualityStandard: '#quality-standard',
                qualityProfile : '#quality-profile',
                qualitySize    : '#quality-size'
            },

            ui: {

            },

            events: {

            },

            initialize: function (options) {
                this.settings = options.settings;
                qualityProfileCollection.fetch();
                this.qualitySizeCollection = new NzbDrone.Quality.QualitySizeCollection();
                this.qualitySizeCollection.fetch();
            },

            onRender: function () {
                this.qualityStandard.show(new NzbDrone.Settings.Quality.QualityView({model: this.settings, qualityProfiles: qualityProfileCollection}));
                this.qualityProfile.show(new NzbDrone.Settings.Quality.Profile.QualityProfileCollectionView({collection: qualityProfileCollection}));
                this.qualitySize.show(new NzbDrone.Settings.Quality.Size.QualitySizeCollectionView({collection: this.qualitySizeCollection}));
            }
        });
    });

