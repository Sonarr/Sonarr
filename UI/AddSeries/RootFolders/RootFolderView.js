"use strict";

define(['app', 'AddSeries/RootFolders/RootFolderCollection', 'Mixins/AutoComplete'], function (app, rootFolders) {

    NzbDrone.AddSeries.RootFolderItemView = Backbone.Marionette.ItemView.extend({

        template: 'AddSeries/RootFolders/RootFolderItemTemplate',
        tagName : 'tr',

        events: {
            'click .x-remove': 'removeFolder',
            'click .x-folder': 'folderSelected'
        },

        removeFolder: function () {
            this.model.destroy({ wait: true });
            this.model.collection.remove(this.model);
        },

        folderSelected: function () {
            this.trigger('folderSelected', this.model);
        }
    });

    NzbDrone.AddSeries.RootDirListView = Backbone.Marionette.CollectionView.extend({
        itemView: NzbDrone.AddSeries.RootFolderItemView,

        tagName  : 'table',
        className: 'table table-hover'
    });

    NzbDrone.AddSeries.RootFolders.Layout = Backbone.Marionette.Layout.extend({
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
            this.collection = rootFolders;
            this.rootfolderListView = new NzbDrone.AddSeries.RootDirListView({ collection: rootFolders });
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
            var newDir = new NzbDrone.AddSeries.RootFolders.RootFolderModel(
                {
                    Path: this.ui.pathInput.val()
                });


            rootFolders.create(newDir, {
                wait: true, success: function () {
                    rootFolders.fetch();
                }
            });
        }
    });

});
