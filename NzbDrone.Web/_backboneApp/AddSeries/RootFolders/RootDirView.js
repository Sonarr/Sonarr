'use strict;'
/// <reference path="../../app.js" />
/// <reference path="RootDirModel.js" />
/// <reference path="RootDirCollection.js" />

NzbDrone.AddSeries.RootDirItemView = Backbone.Marionette.ItemView.extend({

    template: 'AddSeries/RootFolders/RootDirItemTemplate',
    tagName: 'tr',

    events: {
        'click .x-remove': 'removeFolder',
    },

    onRender: function () {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    },

    removeFolder: function () {
        this.model.destroy({ wait: true });
        this.model.collection.remove(this.model);
    },

});

NzbDrone.AddSeries.RootDirListView = Backbone.Marionette.CollectionView.extend({
    itemView: NzbDrone.AddSeries.RootDirItemView,

    tagName: 'table',
    className: 'table table-hover',
});

NzbDrone.AddSeries.RootDirView = Backbone.Marionette.Layout.extend({
    template: 'AddSeries/RootFolders/RootDirTemplate',
    route: 'series/add/rootdir',

    ui: {
        pathInput: '.x-path input'
    },

    regions: {
        currentDirs: '#current-dirs',
    },

    events: {
        'click .x-add': 'addFolder',
    },


    collection: new NzbDrone.AddSeries.RootDirCollection(),

    onRender: function () {

        this.currentDirs.show(new NzbDrone.AddSeries.RootDirListView({ collection: this.collection }));
        this.collection.fetch();
    },


    addFolder: function () {
        var newDir = new NzbDrone.AddSeries.RootDirModel(
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
            context.collection.fetch({ data: $.param({ term: term }) });
        }


    },
});