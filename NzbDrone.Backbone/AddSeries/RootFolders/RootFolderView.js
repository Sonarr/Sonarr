'use strict;'

define(['app', 'AddSeries/RootFolders/RootFolderCollection', 'Shared/AutoComplete'], function (app,rootFolders) {


    NzbDrone.AddSeries.RootFolderItemView = Backbone.Marionette.ItemView.extend({

        template: 'AddSeries/RootFolders/RootFolderItemTemplate',
        tagName: 'tr',

        events: {
            'click .x-remove': 'removeFolder'
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        },

        removeFolder: function () {
            this.model.destroy({ wait: true });
            this.model.collection.remove(this.model);
        }

    });

    NzbDrone.AddSeries.RootDirListView = Backbone.Marionette.CollectionView.extend({
        itemView: NzbDrone.AddSeries.RootFolderItemView,

        tagName: 'table',
        className: 'table table-hover'
    });

    NzbDrone.AddSeries.RootDirView = Backbone.Marionette.Layout.extend({
        template: 'AddSeries/RootFolders/RootFolderTemplate',
        route: 'series/add/rootdir',

        ui: {
            pathInput: '.x-path input'
        },

        regions: {
            currentDirs: '#current-dirs'
        },

        events: {
            'click .x-add': 'addFolder'
        },


        onRender: function () {

            this.collection = rootFolders;
            this.currentDirs.show(new NzbDrone.AddSeries.RootDirListView({ collection: this.collection }));
            this.ui.pathInput.autoComplete('/directories');
        },


        addFolder: function () {
            var newDir = new NzbDrone.AddSeries.RootFolders.RootFolderModel(
            {
                Path: this.ui.pathInput.val()
            });

            var self = this;

            this.collection.create(newDir, {
                wait: true, success: function () {
                    self.collection.fetch();
                }
            });
        },

        search: function (context) {

            var term = context.ui.seriesSearch.val();

            if (term === "") {
                context.collection.reset();
            } else {
                console.log(term);
                context.collection.fetch({ data: { term: term } });
            }


        }
    });

});
