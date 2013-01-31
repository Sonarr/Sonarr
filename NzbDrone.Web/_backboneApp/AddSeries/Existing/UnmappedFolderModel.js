'use strict';
/// <reference path="../../app.js" />
NzbDrone.AddSeries.Existing.UnmappedFolderModel = Backbone.Model.extend({


});

NzbDrone.AddSeries.Existing.UnmappedFolderCollection = Backbone.Collection.extend({
    model: NzbDrone.AddSeries.Existing.UnmappedFolderModel,


    importArray: function (unmappedFolderArray) {

        if (!unmappedFolderArray) {
            throw "folder array is required";
        }

        _.each(unmappedFolderArray, function (folder) {
            this.push(new NzbDrone.AddSeries.Existing.UnmappedFolderModel({ folder: folder }));
        }, this);
    }
});



