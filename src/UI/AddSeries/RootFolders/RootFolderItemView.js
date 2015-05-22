var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template  : 'AddSeries/RootFolders/RootFolderItemViewTemplate',
    className : 'recent-folder',
    tagName   : 'tr',

    initialize : function() {
        this.listenTo(this.model, 'change', this.render);
    },

    events : {
        'click .x-delete' : 'removeFolder',
        'click .x-folder' : 'folderSelected'
    },

    removeFolder : function() {
        var self = this;

        this.model.destroy().success(function() {
            self.close();
        });
    },

    folderSelected : function() {
        this.trigger('folderSelected', this.model);
    }
});