/// <reference path="../../app.js" />
/// <reference path="../SearchResultModel.js" />
/// <reference path="../SearchResultCollection.js" />

NzbDrone.AddSeries.SearchItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/AddNewSeries/SearchResultTemplate",
    className: 'row',

    onRender: function () {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

});

NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

    className: 'result',
    itemView: NzbDrone.AddSeries.SearchItemView,

    initialize: function () {
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

        this.searchResult.show(new NzbDrone.AddSeries.SearchResultView({ collection: this.collection }));
    },

    search: function (context) {

        var term = context.ui.seriesSearch.val();

        if (term == "") {
            context.collection.reset();
        } else {
            console.log(term);
            context.collection.fetch({ data: $.param({ term: term }) });
        }


    },
});