'use strict';
define(
    [
        'app',
        'marionette',
        'Episode/Search/ButtonsView',
        'Episode/Search/ManualLayout',
        'Release/Collection',
        'Series/SeriesCollection',
        'Commands/CommandController',
        'Shared/LoadingView'
    ], function (App, Marionette, ButtonsView, ManualSearchLayout, ReleaseCollection, SeriesCollection,CommandController, LoadingView) {

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

                App.vent.trigger(App.Commands.CloseModalCommand);
            },

            _searchManual: function (e) {
                if (e) {
                    e.preventDefault();
                }

                var self = this;

                this.mainView = new LoadingView();
                this._showMainView();

                var releases = new ReleaseCollection();
                var promise = releases.fetchEpisodeReleases(this.model.id);

                promise.done(function () {
                    if (!self.isClosed) {
                        self.mainView = new ManualSearchLayout({collection: releases});
                        self._showMainView();
                    }
                });
            },

            _showMainView: function () {
                this.main.show(this.mainView);
            },

            _showButtons: function () {
                this.mainView = new ButtonsView();
                this._showMainView();
            }
        });

    });
