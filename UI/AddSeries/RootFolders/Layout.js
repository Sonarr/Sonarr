'use strict';

define(
    [
        'marionette',
        'AddSeries/RootFolders/CollectionView',
        'AddSeries/RootFolders/Collection',
        'AddSeries/RootFolders/Model',
        'Shared/LoadingView',
        'Mixins/AutoComplete'
    ], function (Marionette, RootFolderCollectionView, RootFolderCollection, RootFolderModel, LoadingView) {

        return Marionette.Layout.extend({
            template: 'AddSeries/RootFolders/LayoutTemplate',

            ui: {
                pathInput: '.x-path input'
            },

            regions: {
                currentDirs: '#current-dirs'
            },

            events: {
                'click .x-add': '_addFolder'
            },

            initialize: function () {
                this.collection = RootFolderCollection;
                this.rootfolderListView = new RootFolderCollectionView({ collection: RootFolderCollection });

                this.listenTo(this.rootfolderListView, 'itemview:folderSelected', this._onFolderSelected);
            },

            onRender: function () {
                var self = this;
                this.currentDirs.show(new LoadingView());

                RootFolderCollection.promise.done(function () {
                    self.currentDirs.show(self.rootfolderListView);
                });

                this.ui.pathInput.autoComplete('/directories');
            },

            _onFolderSelected: function (options) {
                this.trigger('folderSelected', options);
            },

            _addFolder: function () {

                var self = this;

                var newDir = new RootFolderModel({
                    Path: this.ui.pathInput.val()
                });

                RootFolderCollection.add(newDir);

                newDir.save().done(function () {
                    self.trigger('folderSelected', {model: newDir});
                });
            }
        });

    });
