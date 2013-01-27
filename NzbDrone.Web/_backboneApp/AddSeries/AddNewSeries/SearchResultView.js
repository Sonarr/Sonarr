/// <reference path="../../app.js" />
/// <reference path="../SearchResultModel.js" />
/// <reference path="../SearchResultCollection.js" />

NzbDrone.AddSeries.SearchItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/AddNewSeries/SearchResultTemplate",
    className: 'search-item accordion-group',
    onRender: function () {
        this.listenTo(this.model, 'change', this.render);
        //this.listenTo(this.model.get('rootFolders'), 'reset', this.render);
        //this.listenTo(this.model.get('qualityProfiles'), 'reset', this.render);
    }

});

NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

    itemView: NzbDrone.AddSeries.SearchItemView,

    className: 'accordion',

    initialize: function () {
        this.listenTo(this.collection, 'reset', this.render);
    },

});
