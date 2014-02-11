'use strict';

define(
    [
        'marionette',
        'Quality/QualityProfileCollection',
        'Settings/Quality/Profile/QualityProfileCollectionView',
        'Quality/QualityDefinitionCollection',
        'Settings/Quality/Definition/QualityDefinitionCollectionView'
    ], function (Marionette, QualityProfileCollection, QualityProfileCollectionView, QualityDefinitionCollection, QualityDefinitionCollectionView) {
        return Marionette.Layout.extend({
            template: 'Settings/Quality/QualityLayoutTemplate',

            regions: {
                qualityProfile    : '#quality-profile',
                qualityDefinition : '#quality-definition'
            },

            initialize: function (options) {
                this.settings = options.settings;
                QualityProfileCollection.fetch();
                this.qualityDefinitionCollection = new QualityDefinitionCollection();
                this.qualityDefinitionCollection.fetch();
            },

            onShow: function () {
                this.qualityProfile.show(new QualityProfileCollectionView({collection: QualityProfileCollection}));
                this.qualityDefinition.show(new QualityDefinitionCollectionView({collection: this.qualityDefinitionCollection}));
            }
        });
    });

