﻿'use strict';
define(
    [
        'Settings/Indexers/Model'
    ], function (IndexerModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/indexer',
            model: IndexerModel
        });
    });
