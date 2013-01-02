QualityTypeApp = {};

QualityTypeApp.Views = {};
QualityTypeApp.Models = {};
QualityTypeApp.Collections = {};

QualityTypeApp.App = new Backbone.Marionette.Application();

// Setup default application views
QualityTypeApp.App.addInitializer(function () {

    QualityTypeApp.App.addRegions({
        mainRegion: '#sliders'
    });

    var qualityTypes = new QualityTypeCollectionView();
    
    QualityTypeApp.App.mainRegion.show(qualityTypes);
});