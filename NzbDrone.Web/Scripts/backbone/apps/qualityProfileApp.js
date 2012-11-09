QualityProfileApp = {};

QualityProfileApp.Views = {};
QualityProfileApp.Models = {};
QualityProfileApp.Collections = {};

QualityProfileApp.App = new Backbone.Marionette.Application();

// Setup default application views
QualityProfileApp.App.addInitializer(function () {

    QualityProfileApp.App.addRegions({
        mainRegion: '#profiles'
    });

    var qualityProfiles = new QualityProfileCollectionView();
    
    QualityProfileApp.App.mainRegion.show(qualityProfiles);
});