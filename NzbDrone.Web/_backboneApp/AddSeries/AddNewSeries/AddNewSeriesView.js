/// <reference path="../../app.js" />
/// <reference path="../SearchResultModel.js" />
/// <reference path="../SearchResultCollection.js" />

NzbDrone.AddSeries.SearchItemView = Backbone.Marionette.ItemView.extend({

    tagName: 'li',
    template: "AddSeries/AddNewSeries/SearchResultTemplate",
    itemView: NzbDrone.AddSeries.SearchResultModel,


    initialize: function () {

        this.collection = new NzbDrone.AddSeries.SearchResultCollection();
        this.bindTo(this.collection, 'reset', this.render);
    },

});

NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

    tagName: 'ul',
    className: 'result',
    itemView: NzbDrone.AddSeries.SearchResultModel,

    collection :  new NzbDrone.AddSeries.SearchResultCollection(),

    initialize: function () {
        //this.collection = new NzbDrone.AddSeries.SearchResultCollection();
        this.listenTo(this.collection, 'reset', this.render);
    },

});

NzbDrone.AddSeries.AddNewSeriesView = Backbone.Marionette.Layout.extend({
    template: "AddSeries/AddNewSeries/AddNewSeriesTemplate",

    ui: {
        seriesSearch: '.search input'
    },

    regions: {
        searchResult: "#search-result",
    },

    collection: new NzbDrone.AddSeries.SearchResultCollection(),

    onRender: function () {

        console.log('binding auto complete');
        var self = this;

        this.ui.seriesSearch
            .data('timeout', null)
            .keyup(function () {
                clearTimeout(self.$el.data('timeout'));
                self.$el.data('timeout', setTimeout(self.search, 500, self));
            });
        
        this.searchResult.show(new NzbDrone.AddSeries.SearchResultView());
    },
    
    search: function (context) {

        var term = context.ui.seriesSearch.val();
        console.log(term);
        context.collection.fetch({ data: $.param({ term: term }) });
    },
});