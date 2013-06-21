'use strict';
define(
    [
        'backbone',
        'AddSeries/Existing/UnmappedFolderModel'
    ], function (Backbone, UnmappedFolderModel) {
        return Backbone.Collection.extend({
            model: UnmappedFolderModel,

            importItems: function (rootFolderModel) {

                this.reset();
                var rootFolder = rootFolderModel;

                _.each(rootFolderModel.get('unmappedFolders'), function (folder) {
                    this.push(new UnmappedFolderModel({
                        rootFolder: rootFolder,
                        folder    : folder
                    }));
                }, this);
            }
        });
    });
