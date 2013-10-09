'use strict';
define(
    [
        'backbone',
        'Settings/Indexers/Model',
    ], function (Backbone, IndexerModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/indexer',
            model: IndexerModel
        });
    });
