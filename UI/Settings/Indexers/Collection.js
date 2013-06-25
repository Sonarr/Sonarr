﻿'use strict';
define(
    [
        'Settings/Indexers/Model',
        'Form/FormBuilder'
    ], function (IndexerModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/indexer',
            model: IndexerModel
        });
    });
