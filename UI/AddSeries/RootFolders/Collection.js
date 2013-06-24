﻿'use strict';
define(
    [
        'backbone',
        'AddSeries/RootFolders/Model',
        'mixins/backbone.signalr.mixin'
    ], function (Backbone, RootFolderModel) {

        var RootFolderCollection = Backbone.Collection.extend({
            url  : window.ApiRoot + '/rootfolder',
            model: RootFolderModel
        });

        var collection = new RootFolderCollection().BindSignalR();

        return collection;
    });
