'use strict';
define(
    [
        'backbone',
        'AddSeries/RootFolders/Model',
        'mixins/backbone.signalr.mixin'
    ], function (Backbone, RootFolderModel) {

        var rootFolderCollection = Backbone.Collection.extend({
            url  : NzbDrone.Constants.ApiRoot + '/rootfolder',
            model: RootFolderModel
        });

        var collection = new rootFolderCollection().BindSignalR();

        return collection;
    });
