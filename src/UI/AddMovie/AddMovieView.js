var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var AddMovieCollection = require('./AddMovieCollection');
var SearchResultCollectionView = require('./SearchResultCollectionView');
var EmptyView = require('./EmptyView');
var NotFoundView = require('./NotFoundView');
var ErrorView = require('./ErrorView');
var LoadingView = require('../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'AddMovie/AddMovieViewTemplate',

    regions : {
        searchResult : '#search-result'
    },

    ui : {
        movieSearch : '.x-movie-search',
        searchBar    : '.x-search-bar',
        loadMore     : '.x-load-more'
    },

    events : {
        'click .x-load-more' : '_onLoadMore'
    },
	
    initialize : function(options) {
        this.isExisting = options.isExisting;
        this.collection = new AddMovieCollection();

        if (this.isExisting) {
            this.collection.unmappedFolderModel = this.model;
        }

        if (this.isExisting) {
            this.className = 'existing-movies';
        } else {
            this.className = 'new-movie';
        }

        this.listenTo(vent, vent.Events.MovieAdded, this._onMovieAdded);
        this.listenTo(this.collection, 'sync', this._showResults);

        this.resultCollectionView = new SearchResultCollectionView({
            collection : this.collection,
            isExisting : this.isExisting
        });

        this.throttledSearch = _.debounce(this.search, 1000, { trailing : true }).bind(this);
    },
	
    onRender : function() {
        var self = this;

        this.$el.addClass(this.className);

        this.ui.movieSearch.keyup(function(e) {

            if (_.contains([
                    9,
                    16,
                    17,
                    18,
                    19,
                    20,
                    33,
                    34,
                    35,
                    36,
                    37,
                    38,
                    39,
                    40,
                    91,
                    92,
                    93
                ], e.keyCode)) {
                return;
            }

            self._abortExistingSearch();
            self.throttledSearch({
                term : self.ui.movieSearch.val()
            });
        });
		
        this._clearResults();

        if (this.isExisting) {
            this.ui.searchBar.hide();
        }
	},
	
    search : function(options) {
        var self = this;

        this.collection.reset();

        if (!options.term || options.term === this.collection.term) {
            return Marionette.$.Deferred().resolve();
        }

        this.searchResult.show(new LoadingView());
        this.collection.term = options.term;
        this.currentSearchPromise = this.collection.fetch({
            data : { term : options.term }
        });

        this.currentSearchPromise.fail(function() {
            self._showError();
        });

        return this.currentSearchPromise;
    },
	
    _abortExistingSearch : function() {
        if (this.currentSearchPromise && this.currentSearchPromise.readyState > 0 && this.currentSearchPromise.readyState < 4) {
            console.log('aborting previous pending search request.');
            this.currentSearchPromise.abort();
        } else {
            this._clearResults();
        }
    },
	
    _showError : function() {
        if (!this.isClosed) {
            this.ui.searchBar.show();
            this.searchResult.show(new ErrorView({ term : this.collection.term }));
            this.collection.term = '';
        }
    },
	
    _clearResults : function() {
        if (!this.isExisting) {
            this.searchResult.show(new EmptyView());
        } else {
            this.searchResult.close();
        }
    },
	
	_showResults : function() {
        if (!this.isClosed) {
            if (this.collection.length === 0) {
                this.ui.searchBar.show();
                this.searchResult.show(new NotFoundView({ term : this.collection.term }));
            } else {
                this.searchResult.show(this.resultCollectionView);
                if (!this.showingAll && this.isExisting) {
                    this.ui.loadMore.show();
                }
            }
        }
    },
	
	_onMovieAdded : function(options) {
        if (this.isExisting && options.movie.get('path') === this.model.get('folder').path) {
            this.close();
        }

        else if (!this.isExisting) {
            this.collection.term = '';
            this.collection.reset();
            this._clearResults();
            this.ui.movieSearch.val('');
            this.ui.movieSearch.focus();
        }
    },

    _onLoadMore : function() {
        var showingAll = this.resultCollectionView.showMore();
        this.ui.searchBar.show();

        if (showingAll) {
            this.ui.loadMore.hide();
        }
    }

});
