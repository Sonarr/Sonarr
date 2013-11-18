'use strict';
define(
    [
        'vent',
        'marionette',
        'Episode/Search/ButtonsView',
        'Episode/Search/ManualLayout',
        'Release/ReleaseCollection',
        'Series/SeriesCollection',
        'Commands/CommandController',
        'Shared/LoadingView'
    ], function (vent, Marionette, ButtonsView, ManualSearchLayout, ReleaseCollection, SeriesCollection,CommandController, LoadingView) {

        return Marionette.Layout.extend({
            template: 'Episode/Search/EpisodeSearchLayoutTemplate',

            regions: {
                main: '#episode-search-region'
            },

            events: {
                'click .x-search-auto'  : '_searchAuto',
                'click .x-search-manual': '_searchManual',
                'click .x-search-back'  : '_showButtons'
            },

            initialize: function () {
                this.mainView = new ButtonsView();
                this.releaseCollection = new ReleaseCollection();

                this.listenTo(this.releaseCollection, 'sync', this._showSearchResults);
            },

            onShow: function () {
                if (this.startManualSearch) {
                    this._searchManual();
                }

                else {
                    this._showMainView();
                }
            },

            _searchAuto: function (e) {
                if (e) {
                    e.preventDefault();
                }

                CommandController.Execute('episodeSearch', {
                    episodeIds: [ this.model.get('id') ]
                });

                vent.trigger(vent.Commands.CloseModalCommand);
            },

            _searchManual: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.mainView = new LoadingView();
                this._showMainView();
                this.releaseCollection.fetchEpisodeReleases(this.model.id);
            },

            _showMainView: function () {
                this.main.show(this.mainView);
            },

            _showButtons: function () {
                this.mainView = new ButtonsView();
                this._showMainView();
            },

            _showSearchResults: function () {
                this.mainView = new ManualSearchLayout({ collection: this.releaseCollection });
                this._showMainView();
            }
        });
    });
