﻿'use strict';
define(
    [
        'Settings/Indexers/Model',
        'Form/FormBuilder'
    ], function (IndexerModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/indexer',
            model: IndexerModel
        });
    });
