'use strict';

define(
    [
        'marionette',
        'Quality/QualityDefinitionCollection',
        'Settings/Quality/Definition/QualityDefinitionCollectionView'
    ], function (Marionette, QualityDefinitionCollection, QualityDefinitionCollectionView) {
        return Marionette.Layout.extend({
            template: 'Settings/Quality/QualityLayoutTemplate',

            regions: {
                qualityDefinition : '#quality-definition'
            },

            initialize: function (options) {
                this.settings = options.settings;
                this.qualityDefinitionCollection = new QualityDefinitionCollection();
                this.qualityDefinitionCollection.fetch();
            },

            onShow: function () {
                this.qualityDefinition.show(new QualityDefinitionCollectionView({collection: this.qualityDefinitionCollection}));
            }
        });
    });

