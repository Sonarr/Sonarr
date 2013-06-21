'use strict';
define(
    [
        'marionette',
        'AddSeries/Existing/CompositeView',
        'AddSeries/Existing/UnmappedFolderCollection'
    ], function (Marionette, UnmappedFolderCompositeView, UnmappedFolderCollection) {

        return Marionette.CollectionView.extend({

            itemView: UnmappedFolderCompositeView,

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
                    this.addItemView(model, this.getItemView(), index);
                    $.when(this.children.findByModel(model).search()).then(function () {
                        that._showAndSearch(currentIndex + 1);
                    });
                }
            }

        });
    });
