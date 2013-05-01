"use strict";
define(['app', 'Settings/Indexers/Model'], function () {
    NzbDrone.Settings.Indexers.Collection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/indexer',
        model: NzbDrone.Settings.Indexers.Model
    });
});
