'use strict';
define(
    [
        'marionette',
        'Episode/Summary/EpisodeSummaryLayout',
        'Episode/Search/EpisodeSearchLayout',
        'Episode/Activity/EpisodeActivityLayout',
        'Series/SeriesCollection'
    ], function (Marionette, SummaryLayout, SearchLayout, EpisodeActivityLayout, SeriesCollection) {

        return Marionette.Layout.extend({
            template: 'Episode/EpisodeDetailsLayoutTemplate',

            regions: {
                summary : '#episode-summary',
                activity: '#episode-activity',
                search  : '#episode-search'
            },

            ui: {
                summary  : '.x-episode-summary',
                activity : '.x-episode-activity',
                search   : '.x-episode-search',
                monitored: '.x-episode-monitored'
            },

            events: {

                'click .x-episode-summary'  : '_showSummary',
                'click .x-episode-activity' : '_showActivity',
                'click .x-episode-search'   : '_showSearch',
                'click .x-episode-monitored': '_toggleMonitored'
            },

            templateHelpers: {},

            initialize: function (options) {
                this.templateHelpers.hideSeriesLink = options.hideSeriesLink;

                this.series = SeriesCollection.find({ id: this.model.get('seriesId') });
                this.templateHelpers.series = this.series.toJSON();
                this.openingTab = options.openingTab || 'summary';
            },

            onShow: function () {
                this.searchLayout = new SearchLayout({ model: this.model });

                if (this.openingTab === 'search') {
                    this.searchLayout.startManualSearch = true;
                    this._showSearch();
                }

                else {
                    this._showSummary();
                }

                this._setMonitoredState();
            },

            _showSummary: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.summary.tab('show');
                this.summary.show(new SummaryLayout({model: this.model, series: this.series}));
            },

            _showActivity: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.activity.tab('show');
                this.activity.show(new EpisodeActivityLayout({model: this.model, series: this.series}));
            },

            _showSearch: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.search.tab('show');
                this.search.show(this.searchLayout);
            },

            _toggleMonitored: function () {
                var self = this;
                var name = 'monitored';
                this.model.set(name, !this.model.get(name), { silent: true });

                this.ui.monitored.addClass('icon-spinner icon-spin');

                var promise = this.model.save();

                promise.always(function () {
                    self._setMonitoredState();
                });
            },

            _setMonitoredState: function () {
                var monitored = this.model.get('monitored');

                this.ui.monitored.removeClass('icon-spin icon-spinner');

                if (this.model.get('monitored')) {
                    this.ui.monitored.addClass('icon-bookmark');
                    this.ui.monitored.removeClass('icon-bookmark-empty');
                }
                else {
                    this.ui.monitored.addClass('icon-bookmark-empty');
                    this.ui.monitored.removeClass('icon-bookmark');
                }
            }
        });
    });
