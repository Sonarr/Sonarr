'use strict';
define(
    [
        'marionette',
        'Episode/Summary/View',
        'Episode/Search/Layout'
    ], function (Marionette, SummaryView, SearchLayout) {

        return Marionette.Layout.extend({
            template: 'Episode/LayoutTemplate',

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

            onShow: function () {
                this._showSummary();
                this.searchLayout = new SearchLayout({ model: this.model });
                this._setMonitoredState();
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
