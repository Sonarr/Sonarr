'use strict';

define(
    [
        'marionette',
        'AddSeries/RootFolders/RootFolderCollectionView',
        'AddSeries/RootFolders/RootFolderCollection',
        'AddSeries/RootFolders/RootFolderModel',
        'Shared/LoadingView',
        'Mixins/AsValidatedView',
        'Mixins/AutoComplete'
    ], function (Marionette, RootFolderCollectionView, RootFolderCollection, RootFolderModel, LoadingView, AsValidatedView) {

        var layout = Marionette.Layout.extend({
            template: 'AddSeries/RootFolders/RootFolderLayoutTemplate',

            ui: {
                pathInput: '.x-path input'
            },

            regions: {
                currentDirs: '#current-dirs'
            },

            events: {
                'click .x-add': '_addFolder',
                'keydown .x-path input': '_keydown'
            },

            initialize: function () {
                this.collection = RootFolderCollection;
                this.rootfolderListView = new RootFolderCollectionView({ collection: RootFolderCollection });

                this.listenTo(this.rootfolderListView, 'itemview:folderSelected', this._onFolderSelected);
                this.listenTo(RootFolderCollection, 'sync', this._showCurrentDirs);
            },

            onRender: function () {
                this.currentDirs.show(new LoadingView());

                if (RootFolderCollection.synced) {
                    this._showCurrentDirs();
                }

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

                this.bindToModelValidation(newDir);

                newDir.save().done(function () {
                    RootFolderCollection.add(newDir);
                    self.trigger('folderSelected', {model: newDir});
                });
            },

            _showCurrentDirs: function () {
                this.currentDirs.show(this.rootfolderListView);
            },

            _keydown: function (e) {
                if (e.keyCode !== 13) {
                    return;
                }

                this._addFolder();
            }
        });

        return AsValidatedView.apply(layout);
    });
