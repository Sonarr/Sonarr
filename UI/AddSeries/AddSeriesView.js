'use strict';
define(
    [
        'app',
        'marionette',
        'AddSeries/Collection',
        'AddSeries/SearchResultCollectionView',
        'AddSeries/NotFoundView',
        'Shared/LoadingView',
        'underscore'
    ], function (App, Marionette, AddSeriesCollection, SearchResultCollectionView, NotFoundView, LoadingView, _) {
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

            initialize: function (options) {
                this.isExisting = options.isExisting;
                this.collection = new AddSeriesCollection();

                if (this.isExisting) {
                    this.collection.unmappedFolderModel = this.model;
                }

                if (this.isExisting) {
                    this.className = 'existing-series';
                    this.listenTo(App.vent, App.Events.SeriesAdded, this._onSeriesAdded);
                }
                else {
                    this.className = 'new-series';
                }

                this.listenTo(this.collection, 'sync', this._showResults);

                this.resultCollectionView = new SearchResultCollectionView({
                    collection: this.collection,
                    isExisting: this.isExisting
                });

                this.throttledSearch = _.throttle(this.search, 1000, {trailing: true}).bind(this);
            },

            _onSeriesAdded: function (options) {
                if (this.isExisting && options.series.get('path') === this.model.get('folder').path) {
                    this.close();
                }
            },

            _onLoadMore: function () {
                var showingAll = this.resultCollectionView.showMore();

                if (showingAll) {
                    this.ui.loadMore.hide();
                    this.ui.searchBar.show();
                }
            },

            onRender: function () {
                var self = this;

                this.$el.addClass(this.className);

                this.ui.seriesSearch.keyup(function () {
                    self.searchResult.close();
                    self._abortExistingSearch();
                    self.throttledSearch({
                        term: self.ui.seriesSearch.val()
                    })
                });

                if (this.isExisting) {
                    this.ui.searchBar.hide();
                }
            },

            onShow: function () {
                this.searchResult.show(this.resultCollectionView);
            },

            search: function (options) {

                this.collection.reset();

                if (!options.term || options.term === this.collection.term) {
                    return;
                }

                this.searchResult.show(new LoadingView());
                this.collection.term = options.term;
                this.currentSearchPromise = this.collection.fetch({
                    data: { term: options.term }
                });
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

            _abortExistingSearch: function () {
                if (this.currentSearchPromise && this.currentSearchPromise.readyState > 0 && this.currentSearchPromise.readyState < 4) {
                    console.log('aborting previous pending search request.');
                    this.currentSearchPromise.abort();
                }
            }
        });
    });
