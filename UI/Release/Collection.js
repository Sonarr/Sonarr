"use strict";
define(['app', 'Release/Model'], function () {
    NzbDrone.Release.Collection = Backbone.PageableCollection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/release',
        model: NzbDrone.Release.Model,

        mode : 'client'
    });
});
