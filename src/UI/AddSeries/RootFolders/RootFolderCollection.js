'use strict';
define(
    [
        'backbone',
        'AddSeries/RootFolders/RootFolderModel',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, RootFolderModel) {

        var RootFolderCollection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/rootfolder',
            model: RootFolderModel
        });

        //var collection = new RootFolderCollection().bindSignalR();

        return new RootFolderCollection();
    });
