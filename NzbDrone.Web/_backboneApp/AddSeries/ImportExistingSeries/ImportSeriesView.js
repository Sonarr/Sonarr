'use strict';
/*global NzbDrone, Backbone*/

/// <reference path="../../app.js" />
/// <reference path="../../Series/SeriesModel.js" />
/// <reference path="../SearchResultCollection.js" />
NzbDrone.AddSeries.ExistingFolderItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/ImportExistingSeries/ImportSeriesTemplate",

    events: {
        //'click .x-add': 'add'
    }
});

NzbDrone.AddSeries.ExistingFolderListView = Backbone.Marionette.CollectionView.extend({

    itemView: NzbDrone.AddSeries.ExistingFolderItemView,

    initialize: function () {

        if (this.collection === undefined) {
            throw "root folder collection is required.";
        }

        this.listenTo(this.collection, 'reset', this.render, this);
    }
});
