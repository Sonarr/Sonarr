var Marionette = require('marionette');
var NamingView = require('./Naming/NamingView');
var MovieNamingView = require('./MovieNaming/MovieNamingView');
var SortingView = require('./Sorting/SortingView');
var FileManagementView = require('./FileManagement/FileManagementView');
var PermissionsView = require('./Permissions/PermissionsView');

module.exports = Marionette.Layout.extend({
    template : 'Settings/MediaManagement/MediaManagementLayoutTemplate',

    regions : {
        episodeNaming  : '#episode-naming',
        movieNaming    : '#movie-naming',
        sorting        : '#sorting',
        fileManagement : '#file-management',
        permissions    : '#permissions'
    },

    initialize : function(options) {
        this.settings = options.settings;
        this.namingSettings = options.namingSettings;
    },

    onShow : function() {
        this.episodeNaming.show(new NamingView({ model : this.namingSettings }));
        this.movieNaming.show(new MovieNamingView({ model : this.namingSettings }));
        this.sorting.show(new SortingView({ model : this.settings }));
        this.fileManagement.show(new FileManagementView({ model : this.settings }));
        this.permissions.show(new PermissionsView({ model : this.settings }));
    }
});