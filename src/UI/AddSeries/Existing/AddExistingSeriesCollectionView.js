var Marionette = require('marionette');
var AddSeriesView = require('../AddSeriesView');
var UnmappedFolderCollection = require('./UnmappedFolderCollection');

module.exports = Marionette.CompositeView.extend({
    itemView          : AddSeriesView,
    itemViewContainer : '.x-loading-folders',
    template          : 'AddSeries/Existing/AddExistingSeriesCollectionViewTemplate',

    ui : {
        loadingFolders : '.x-loading-folders'
    },

    initialize : function() {
        this.collection = new UnmappedFolderCollection();
        this.collection.importItems(this.model);
    },

    showCollection : function() {
        this._showAndSearch(0);
    },

    appendHtml : function(collectionView, itemView, index) {
        collectionView.ui.loadingFolders.before(itemView.el);
    },

    _showAndSearch : function(index) {
        var self = this;
        var model = this.collection.at(index);

        if (model) {
            var currentIndex = index;
            var folderName = model.get('folder').name;
            this.addItemView(model, this.getItemView(), index);
            this.children.findByModel(model).search({ term : folderName }).always(function() {
                if (!self.isClosed) {
                    self._showAndSearch(currentIndex + 1);
                }
            });
        }

        else {
            this.ui.loadingFolders.hide();
        }
    },

    itemViewOptions : {
        isExisting : true
    }

});