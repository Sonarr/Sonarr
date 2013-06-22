'use strict';
define(
    [
        'marionette',
        'AddSeries/AddSeriesView',
        'AddSeries/Existing/UnmappedFolderCollection'
    ], function (Marionette, AddSeriesView, UnmappedFolderCollection) {

        return Marionette.CollectionView.extend({

            itemView: AddSeriesView,

            initialize: function () {
                this.collection = new UnmappedFolderCollection();
                this.refreshItems();
            },

            refreshItems: function () {
                this.collection.importItems(this.model);
            },

            showCollection: function () {
                this._showAndSearch(0);
            },

            _showAndSearch: function (index) {

                var model = this.collection.at(index);

                if (model) {
                    var that = this;
                    var currentIndex = index;
                    var folderName = model.get('folder').name;
                    this.addItemView(model, this.getItemView(), index);
                    $.when(this.children.findByModel(model).search({term: folderName})).then(function () {
                        that._showAndSearch(currentIndex + 1);
                    });
                }
            },

            itemViewOptions: {
                isExisting: true
            }

        });
    });
