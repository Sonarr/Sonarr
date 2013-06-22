'use strict';
define([
    'app',
    'marionette',
    'Quality/QualityProfileCollection',
    'Settings/Quality/Profile/QualityProfileCollectionView',
    'Quality/QualitySizeCollection',
    'Settings/Quality/Size/QualitySizeCollectionView'
],
    function (App, Marionette, QualityProfileCollection, QualityProfileCollectionView, QualitySizeCollection, QualitySizeCollectionView) {
        return Marionette.Layout.extend({
            template: 'Settings/Quality/QualityLayoutTemplate',

            regions: {
                qualityStandard: '#quality-standard',
                qualityProfile : '#quality-profile',
                qualitySize    : '#quality-size'
            },

            initialize: function (options) {
                this.settings = options.settings;
                QualityProfileCollection.fetch();
                this.qualitySizeCollection = new QualitySizeCollection();
                this.qualitySizeCollection.fetch();
            },

            onRender: function () {
                this.qualityProfile.show(new QualityProfileCollectionView({collection: QualityProfileCollection}));
                this.qualitySize.show(new QualitySizeCollectionView({collection: this.qualitySizeCollection}));
            }
        });
    });

