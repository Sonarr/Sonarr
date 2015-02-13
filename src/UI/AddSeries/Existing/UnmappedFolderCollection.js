var Backbone = require('backbone');
var UnmappedFolderModel = require('./UnmappedFolderModel');
var _ = require('underscore');

module.exports = Backbone.Collection.extend({
    model : UnmappedFolderModel,

    importItems : function(rootFolderModel) {

        this.reset();
        var rootFolder = rootFolderModel;

        _.each(rootFolderModel.get('unmappedFolders'), function(folder) {
            this.push(new UnmappedFolderModel({
                rootFolder : rootFolder,
                folder     : folder
            }));
        }, this);
    }
});