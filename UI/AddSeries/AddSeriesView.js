'use strict';
define(
    [
        'app',
        'marionette',
        'AddSeries/Collection',
        'AddSeries/SearchResultCollectionView',
        'AddSeries/NotFoundView',
        'Shared/LoadingView',
    ], function (App, Marionette, AddSeriesCollection, SearchResultCollectionView, NotFoundView, LoadingView) {
        return Marionette.Layout.extend({
            template: 'AddSeries/AddSeriesTemplate',

            regions: {
                searchResult: '#search-result'
            },

            ui: {
                seriesSearch: '.x-series-search',
                searchBar   : '.x-search-bar',
                loadMore    : '.x-load-more'
            },

            events: {
                'click .x-load-more': '_onLoadMore'
            },

            _onLoadMore: function () {
                var showingAll = this.resultCollectionView.showMore();

                if (showingAll) {
                    this.ui.loadMore.hide();
                    this.ui.searchBar.show();
                }
            },

            initialize: function (options) {
                this.collection = new AddSeriesCollection({unmappedFolderModel: this.model});
                this.isExisting = options.isExisting;

                if (this.isExisting) {
                    this.className = 'existing-series';
                    this.listenTo(App.vent, App.Events.SeriesAdded, this._onSeriesAdded);
                }
                else {
                    this.className = 'new-series';
                }

                this.listenTo(this.collection, 'sync', this._showResults);
            },

            _onSeriesAdded: function (options) {
                if (options.series.get('path') === this.model.get('folder').path) {
                    this.close();
                }
            },

            onRender: function () {
                var self = this;

                this.$el.addClass(this.className);

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

                this.resultCollectionView = new SearchResultCollectionView({
                    collection: this.collection,
                    isExisting: this.isExisting
                });
            },

            search: function (options) {

                this.abortExistingSearch();

                this.collection.reset();

                if (!options || options.term === '') {
                    this.searchResult.close();
                }
                else {
                    this.searchResult.show(new LoadingView());
                    this.collection.term = options.term;
                    this.currentSearchPromise = this.collection.fetch({
                        data: { term: options.term }
                    });
                }
                return this.currentSearchPromise;
            },

            _showResults: function () {
                if (!this.isClosed) {

                    if (this.collection.length === 0) {
                        this.searchResult.show(new NotFoundView({term: this.collection.term}));
                    }
                    else {
                        this.searchResult.show(this.resultCollectionView);
                        if (!this.showingAll && this.isExisting) {
                            this.ui.loadMore.show();
                        }
                    }
                }
            },

            abortExistingSearch: function () {
                if (this.currentSearchPromise && this.currentSearchPromise.readyState > 0 && this.currentSearchPromise.readyState < 4) {
                    console.log('aborting previous pending search request.');
                    this.currentSearchPromise.abort();
                }
            }
        });
    });
