"use strict";

define(
    [
        'marionette',
        'Settings/Indexers/CollectionView',
        'Settings/Indexers/Options/View'
    ], function (Marionette, CollectionView, OptionsView) {
        return Marionette.Layout.extend({
            template: 'Settings/Indexers/LayoutTemplate',

            regions: {
                indexersRegion : '#indexers-collection',
                indexerOptions        : '#indexer-options'
            },

            initialize: function (options) {
                this.settings = options.settings;
                this.indexersCollection = options.indexersCollection;
            },

            onShow: function () {
                this.indexersRegion.show(new CollectionView({ collection: this.indexersCollection }));
                this.indexerOptions.show(new OptionsView({ model: this.settings }));
            }
        });
    });

