/// <reference path="../../app.js" />
/// <reference path="../SeriesSearchModel.js" />

NzbDrone.AddSeries.AddNewSeriesView = Backbone.Marionette.ItemView.extend({
    template: "AddSeries/AddNewSeries/AddNewSeriesTemplate",

    ui: {
        seriesSearch: '.search input'
    },

    onRender: function() {

        console.log('binding auto complete');
        var self = this;

        this.ui.seriesSearch
            .data('timeout', null)
            .keyup(function() {
                clearTimeout(self.$el.data('timeout'));
                self.$el.data('timeout', setTimeout(self.search, 500, self));
            });
    },

    search: function(context) {
        console.log(context.ui.seriesSearch.val());
    },
});