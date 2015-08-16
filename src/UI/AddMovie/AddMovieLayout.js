var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var RootFolderLayout = require('../Shared/RootFolders/RootFolderLayout');
var AddMovieView = require('./AddMovieView');
var ProfileCollection = require('../Profile/ProfileCollection');
var RootFolderCollection = require('../Shared/RootFolders/RootFolderCollection');
var ExistingMoviesCollectionView = require('./Existing/AddExistingMoviesCollectionView');

module.exports = Marionette.Layout.extend({
    template : 'AddMovie/AddMovieLayoutTemplate',
	
    regions : {
        workspace : '#add-movie-workspace'
    },
	
    events : {
        'click .x-import'  : '_importMovie',
        'click .x-add-new' : '_addMovie'
    },

    attributes : {
        id : 'add-movie-screen'
    },
	
    initialize : function() {
        ProfileCollection.fetch();
        RootFolderCollection.fetch().done(function() {
            RootFolderCollection.synced = true;
        });
    },
	
    onShow : function() {
        this.workspace.show(new AddMovieView());
    },
	
	_addMovie : function() {
        this.workspace.show(new AddMovieView());	
	},
	
    _importMovie : function() {
        this.rootFolderLayout = new RootFolderLayout();
        this.listenTo(this.rootFolderLayout, 'folderSelected', this._folderSelected);
        AppLayout.modalRegion.show(this.rootFolderLayout);
    },
	
	_folderSelected : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);

        this.workspace.show(new ExistingMoviesCollectionView({ model : options.model }));
	}
});