'use strict';
define(
    [
        'app',
        'marionette',
        'Episode/Search/ManualLayout',
        'Release/Collection',
        'Shared/SpinnerView',
        'Shared/Messenger',
        'Commands/CommandController'
    ], function (App, Marionette, ManualSearchLayout, ReleaseCollection, SpinnerView, Messenger, CommandController) {

        return Marionette.Layout.extend({
            template: 'Episode/Search/LayoutTemplate',

            regions: {
                main: '#episode-search-region'
            },

            events: {
                'click .x-search-auto': '_searchAuto',
                'click .x-search-manual': '_searchManual'
            },

            onShow: function () {
                this._releaseSearchActivated = false;
            },

            _searchAuto: function (e) {
                if (e) {
                    e.preventDefault();
                }

                CommandController.Execute('episodeSearch', { episodeId: this.model.get('id') });

                var seriesTitle = this.model.get('series').get('title');
                var season = this.model.get('seasonNumber');
                var episode = this.model.get('episodeNumber');
                var message = seriesTitle + ' - S' + season.pad(2) + 'E' + episode.pad(2);

                Messenger.show({
                    message: 'Search started for: ' + message
                });

                App.modalRegion.closeModal();
            },

            _searchManual: function (e) {
                if (e) {
                    e.preventDefault();
                }

                if (this._releaseSearchActivated) {
                    return;
                }

                var self = this;

                this.main.show(new SpinnerView());

                var releases = new ReleaseCollection();
                var promise = releases.fetchEpisodeReleases(this.model.id);

                promise.done(function () {
                    if (!self.isClosed) {
                        self.main.show(new ManualSearchLayout({collection: releases}));
                    }
                });
            }
        });

    });
