"use strict";
define(['app', 'Settings/Indexers/Model'], function (App, IndexerModel) {
    return Backbone.Collection.extend({
        url  : App.Constants.ApiRoot + '/indexer',
        model: IndexerModel
    });
});
