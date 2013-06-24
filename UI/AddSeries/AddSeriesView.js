﻿'use strict';
define(
    [
        'marionette',
        'AddSeries/Collection',
        'AddSeries/SearchResultCollectionView',
        'Shared/SpinnerView'
    ], function (Marionette, AddSeriesCollection, SearchResultCollectionView, SpinnerView) {
        return Marionette.Layout.extend({
            template: 'AddSeries/AddSeriesTemplate',

            ui: {
                seriesSearch: '.x-series-search',
                searchBar   : '.x-search-bar',
                loadMore    : '.x-load-more'
            },

            regions: {
                searchResult: '#search-result'
            },

            initialize: function (options) {
                this.collection = new AddSeriesCollection();
                this.isExisting = options.isExisting;
            },

            onRender: function () {
                var self = this;

                this.ui.seriesSearch.data('timeout', null).keyup(function () {
                    window.clearTimeout(self.$el.data('timeout'));
                    self.$el.data('timeout', window.setTimeout(function () {
                        self.search.call(self, {
                            term: self.ui.seriesSearch.val()
                        });
                    }, 500));
                });

                if (this.isExisting) {
                    this.ui.searchBar.hide();
                }

                this.resultView = new SearchResultCollectionView({
                    fullResult: this.collection,
                    isExisting: this.isExisting
                });
            },

            search: function (options) {

                var self = this;

                this.abortExistingRequest();

                this.collection.reset();

                if (!options || options.term === '') {
                    this.searchResult.close();
                }
                else {
                    this.searchResult.show(new SpinnerView());
                    this.currentSearchRequest = this.collection.fetch({
                        data: { term: options.term }
                    }).done(function () {
                            self.searchResult.show(self.resultView);

                            if (!self.showingAll && self.isExisting) {
                                self.ui.loadMore.show();
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
