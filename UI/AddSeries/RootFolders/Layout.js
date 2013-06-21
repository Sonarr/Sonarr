"use strict";

define(
    [
        'marionette',
        'AddSeries/RootFolders/CollectionView',
        'AddSeries/RootFolders/Model',
        'AddSeries/RootFolders/Collection',
        'Mixins/AutoComplete'
    ], function (Marionette, RootFolderCollectionView, RootFolderCollection, RootFolderModel) {

        return Marionette.Layout.extend({
            template: 'AddSeries/RootFolders/LayoutTemplate',

            ui: {
                pathInput: '.x-path input'
            },

            regions: {
                currentDirs: '#current-dirs'
            },

            events: {
                'click .x-add': 'addFolder'
            },

            initialize: function () {
                this.collection = RootFolderCollection;
                this.rootfolderListView = new RootFolderCollectionView({ collection: RootFolderCollection });
                this.rootfolderListView.on('itemview:folderSelected', this._onFolderSelected, this);
            },


            onRender: function () {

                this.currentDirs.show(this.rootfolderListView);
                this.ui.pathInput.autoComplete('/directories');
            },

            _onFolderSelected: function (options) {
                this.trigger('folderSelected', options);
            },

            addFolder: function () {
                var newDir = new RootFolderModel({
                    Path: this.ui.pathInput.val()
                });

                RootFolderCollection.create(newDir, {
                    wait: true, success: function () {
                        RootFolderCollection.fetch();
                    }
                });
            }
        });

    });
