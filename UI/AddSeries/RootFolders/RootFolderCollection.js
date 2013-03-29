"use strict";
define(['app', 'AddSeries/RootFolders/RootFolderModel'], function () {

    var rootFolderCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/rootfolder',
        model: NzbDrone.AddSeries.RootFolders.RootFolderModel
    });

    return new rootFolderCollection();
});