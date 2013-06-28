'use strict';
define(
    [
        'marionette',
        'Episode/Summary/View',
        'Episode/Search/Layout',
        'Release/Collection',
        'Shared/SpinnerView'
    ], function (Marionette, SummaryView, SearchLayout, ReleaseCollection, SpinnerView) {

        return Marionette.Layout.extend({
            template: 'Episode/LayoutTemplate',


            regions: {
                summary : '#episode-summary',
                activity: '#episode-activity',
                search  : '#episode-search'
            },

            ui: {
                summary : '.x-episode-summary',
                activity: '.x-episode-activity',
                search  : '.x-episode-search'
            },

            events: {

                'click .x-episode-summary' : '_showSummary',
                'click .x-episode-activity': '_showActivity',
                'click .x-episode-search'  : '_showSearch'
            },


            onShow: function () {
                this._showSummary();
                this._releaseSearchActivated = false;
            },


            _showSummary: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.summary.tab('show');
                this.summary.show(new SummaryView({model: this.model}));

            },

            _showActivity: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.activity.tab('show');
            },

            _showSearch: function (e) {
                if (e) {
                    e.preventDefault();
                }

                if (this._releaseSearchActivated) {
                    return;
                }

                var self = this;

                this.ui.search.tab('show');
                this.search.show(new SpinnerView());

                var releases = new ReleaseCollection();
                var promise = releases.fetchEpisodeReleases(this.model.id);

                promise.done(function () {
                    if (!self.isClosed) {
                        self.search.show(new SearchLayout({collection: releases}));
                    }
                });
            }

        });

    });
