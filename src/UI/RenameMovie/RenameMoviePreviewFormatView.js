var vent = require('vent');
var Marionette = require('marionette');
var NamingModel = require('../Settings/MediaManagement/Naming/NamingModel');

module.exports = Marionette.ItemView.extend({
    template : 'RenameMovie/RenameMoviePreviewFormatViewTemplate',

    templateHelpers : function() {
        return {
            rename : this.naming.get('renameMovies'),
            format : this.naming.get('standardMovieFormat')
        };
    },

    initialize : function() {
        this.naming = new NamingModel();
        this.naming.fetch();
        this.listenTo(this.naming, 'sync', this.render);
    }
});