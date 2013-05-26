"use strict";
define(['app',
        'AddSeries/RootFolders/RootFolderCollection',
        'AddSeries/SearchResultView',
        'Shared/SpinnerView',
        'AddSeries/Collection'], function () {
    NzbDrone.AddSeries.New.AddNewSeriesView = Backbone.Marionette.Layout.extend({
        template: 'AddSeries/New/AddNewSeriesTemplate',
        route   : 'Series/add/new',

        ui: {
            seriesSearch: '.search input'
        },

        regions: {
            searchResult: '#search-result'
        },

        initialize: function () {

            this.collection = new NzbDrone.AddSeries.Collection();
            NzbDrone.AddSeries.New.AddNewSeriesContext = this;

            NzbDrone.vent.on(NzbDrone.Events.SeriesAdded, function (options) {
                if (!options.existing) {
                    NzbDrone.AddSeries.New.AddNewSeriesContext.ui.seriesSearch.val('');

                    //Todo: Figure out why this is inconsistent
                    options.series.collection.reset();
                }
            });
        },

        onRender: function () {
            var self = this;

            this.ui.seriesSearch
                .data('timeout', null)
                .keyup(function () {
                    window.clearTimeout(self.$el.data('timeout'));
                    self.$el.data('timeout', window.setTimeout(self.search, 500, self));
                });

            this.resultView = new NzbDrone.AddSeries.SearchResultCollectionView({ collection: this.collection });
        },

        search: function (context) {

            context.abortExistingRequest();

            var term = context.ui.seriesSearch.val();
            context.collection.reset();

            if (term === '') {
                context.searchResult.close();
            } else {
                context.searchResult.show(new NzbDrone.Shared.SpinnerView());

                context.currentSearchRequest = context.collection.fetch({
                    data   : { term: term },
                    success: function () {
                        context.searchResult.show(context.resultView);
                    }
                });
            }
        },

        abortExistingRequest: function () {
            if (this.currentSearchRequest && this.currentSearchRequest.readyState > 0 && this.currentSearchRequest.readyState < 4) {
                console.log('aborting previous pending search request.');
                this.currentSearchRequest.abort();
            }
        }
    });
});
