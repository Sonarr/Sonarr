'use strict';
/// <reference path="../../app.js" />
NzbDrone.AddSeries.Existing.UnmappedFolderModel = Backbone.Model.extend({


});

NzbDrone.AddSeries.Existing.UnmappedFolderCollection = Backbone.Collection.extend({
    model: NzbDrone.AddSeries.Existing.UnmappedFolderModel,


    importItems: function (rootFolderModel, quality) {

        if (!rootFolderModel) {
            throw "folder array is required";
        }

        if (!quality) {
            throw "quality is required";
        }

        this.reset();

        var qualityCollection = quality;
        var rootFolder = rootFolderModel.get('path');

        _.each(rootFolderModel.get('unmappedFolders'), function (folder) {
            this.push(new NzbDrone.AddSeries.Existing.UnmappedFolderModel({rootFolder:rootFolder,  folder: folder, quality: qualityCollection }));
        }, this);
    }
});



