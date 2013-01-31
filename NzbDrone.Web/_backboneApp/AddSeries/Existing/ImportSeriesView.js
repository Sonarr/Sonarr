'use strict';
/// <reference path="../../app.js" />
/// <reference path="UnmappedFolderModel.js" />
/// <reference path="../../Series/SeriesModel.js" />
/// <reference path="../SearchResultCollection.js" />


NzbDrone.AddSeries.Existing.FolderMatchResultView = Backbone.Marionette.ItemView.extend({
    template: "AddSeries/Existing/FolderMatchResultViewTemplatate",


});

NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView = Backbone.Marionette.CompositeView.extend({

    template: "AddSeries/Existing/UnmappedFolderCompositeViewTemplatate",
    itemViewContainer: ".x-folder-name-match-results",
    itemView: NzbDrone.AddSeries.Existing.FolderMatchResultView,

    events: {
        'click .x-search': 'search'
    },

    initialize: function () {
        this.collection = new NzbDrone.AddSeries.SearchResultCollection();
    },

    search: function () {

        this.collection.fetch({
            data: $.param({ term: 'simpsons' })
        });
    }

});

NzbDrone.AddSeries.Existing.RootFolderCompositeView = Backbone.Marionette.CompositeView.extend({

    template: "AddSeries/Existing/RootFolderCompositeViewTemplate",
    itemViewContainer: ".x-existing-folder-container",
    itemView: NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView,

    initialize: function () {

        if (!this.model) {
            throw "model is required.";
        }

        this.collection = new NzbDrone.AddSeries.Existing.UnmappedFolderCollection();
        this.collection.importArray(this.model.get('unmappedFolders'));

    },

});

NzbDrone.AddSeries.Existing.ImportSeriesView = Backbone.Marionette.CollectionView.extend({

    itemView: NzbDrone.AddSeries.Existing.RootFolderCompositeView,

    initialize: function () {
        if (!this.collection) {
            throw "root folder collection is required.";
        }
    }
});
