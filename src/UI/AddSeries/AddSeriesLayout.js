var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var ExistingSeriesCollectionView = require('./Existing/AddExistingSeriesCollectionView');
var AddSeriesView = require('./AddSeriesView');
var ProfileCollection = require('../Profile/ProfileCollection');
var RootFolderCollection = require('./RootFolders/RootFolderCollection');
require('../Series/SeriesCollection');

module.exports = Marionette.Layout.extend({
    template : 'AddSeries/AddSeriesLayoutTemplate',

    regions : {
        workspace : '#add-series-workspace'
    },

    events : {
        'click .x-import'  : '_importSeries',
        'click .x-add-new' : '_addSeries'
    },

    attributes : {
        id : 'add-series-screen'
    },

    initialize : function() {
        ProfileCollection.fetch();
        RootFolderCollection.fetch().done(function() {
            RootFolderCollection.synced = true;
        });
    },

    onShow : function() {
        this.workspace.show(new AddSeriesView());
    },

    _folderSelected : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);

        this.workspace.show(new ExistingSeriesCollectionView({ model : options.model }));
    },

    _importSeries : function() {
        this.rootFolderLayout = new RootFolderLayout();
        this.listenTo(this.rootFolderLayout, 'folderSelected', this._folderSelected);
        AppLayout.modalRegion.show(this.rootFolderLayout);
    },

    _addSeries : function() {
        this.workspace.show(new AddSeriesView());
    }
});