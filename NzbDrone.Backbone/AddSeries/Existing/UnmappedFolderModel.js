'use strict';
define(['app', 'Quality/QualityProfileCollection'], function (app, qualityProfiles) {


    NzbDrone.AddSeries.Existing.UnmappedFolderModel = Backbone.Model.extend({

        defaults: {
            quality: qualityProfiles
        }

    });

    NzbDrone.AddSeries.Existing.UnmappedFolderCollection = Backbone.Collection.extend({
        model: NzbDrone.AddSeries.Existing.UnmappedFolderModel,

        importItems: function (rootFolderModel) {

            this.reset();
            var rootFolder = rootFolderModel.get('path');

            _.each(rootFolderModel.get('unmappedFolders'), function (folder) {
                this.push(new NzbDrone.AddSeries.Existing.UnmappedFolderModel({ rootFolder: rootFolder, folder: folder}));
            }, this);
        }
    });
});
