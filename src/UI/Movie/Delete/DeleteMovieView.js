var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Movie/Delete/DeleteMovieTemplate',

    events : {
        'click .x-confirm-delete' : 'removeMovie',
        'change .x-delete-files'  : 'changeDeletedFiles'
    },

    ui : {
        deleteFiles     : '.x-delete-files',
        deleteFilesInfo : '.x-delete-files-info',
        indicator       : '.x-indicator'
    },

    removeMovie : function() {
        var self = this;
        var deleteFiles = this.ui.deleteFiles.prop('checked');
        this.ui.indicator.show();

        this.model.destroy({
            data : { 'deleteFiles' : deleteFiles },
            wait : true
        }).done(function() {
            vent.trigger(vent.Events.MovieDeleted, { movie : self.model });
            vent.trigger(vent.Commands.CloseModalCommand);
        });
    },

    changeDeletedFiles : function() {
        var deleteFiles = this.ui.deleteFiles.prop('checked');

        if (deleteFiles) {
            this.ui.deleteFilesInfo.show();
        } else {
            this.ui.deleteFilesInfo.hide();
        }
    }
});