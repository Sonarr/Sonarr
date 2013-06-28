'use strict';
define(
    [
        'marionette',
        'AddSeries/Collection',
        'AddSeries/SearchResultView'
    ], function (Marionette, AddSeriesCollection, SearchResultView) {

        return Marionette.CompositeView.extend({

            template         : 'AddSeries/Existing/UnmappedFolderCompositeViewTemplate',
            itemViewContainer: '.x-folder-name-match-results',
            itemView         : SearchResultView,

            events: {
                'click .x-btn-search'  : 'search',
                'click .x-load-more'   : '_loadMore',
                'keydown .x-txt-search': 'keyDown'
            },

            ui: {
                searchButton: '.x-btn-search',
                searchText  : '.x-txt-search',
                searchBar   : '.x-search-bar',
                loadMore    : '.x-load-more'
            },

            initialize: function () {
                this.collection = new AddSeriesCollection();

                this.on('item:removed', function () {
                    this.close();
                }, this);
            },

            onRender: function () {
                this.ui.loadMore.show();
            },

            search: function () {
                var icon = this.ui.searchButton.find('icon');
                icon.removeClass('icon-search').addClass('icon-spin icon-spinner disabled');

                var self = this;
                var deferred = $.Deferred();

                this.collection.reset();

                this.searchCollection = new AddSeriesCollection();

                this.searchCollection.fetch({
                    data: { term: this.ui.searchText.val() }
                }).done(function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                        deferred.resolve();
                        self.collection.add(self.searchCollection.shift());

                        if (self.showall) {
                            self._showAll();
                        }

                    }).fail(function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                        deferred.reject();
                    });

                return deferred.promise();
            },


            keyDown: function (e) {
                //Check for enter being pressed
                var code = (e.keyCode ? e.keyCode :e.which);
                if (code === 13) {
                    this.search();
                }
            },

            _loadMore: function () {
                this.showall = true;

                this.ui.searchBar.fadeIn();
                this.ui.loadMore.fadeOut();

                this._showAll();
            },

            _showAll: function () {
                var self = this;
                this.searchCollection.each(function (searchResult) {
                    self.collection.add(searchResult);
                });
            },

            itemViewOptions: function () {
                return {
                    rootFolder: this.model.get('rootFolder'),
                    folder    : this.model.get('folder').path,
                    isExisting: true
                };
            }
        });

    });
