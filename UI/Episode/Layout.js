"use strict";
define(['app', 'Shared/SpinnerView', 'Episode/Summary/View', 'Episode/Search/Layout', 'Release/Collection'], function () {

    NzbDrone.Episode.Layout = Backbone.Marionette.Layout.extend({
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

            'click .x-episode-summary' : 'showSummary',
            'click .x-episode-activity': 'showActivity',
            'click .x-episode-search'  : 'showSearch'
        },


        onShow: function () {
            this.showSummary();
            this._releaseSearchActivated = false;
        },


        showSummary: function (e) {
            if (e) {
                e.preventDefault();
            }

            this.ui.summary.tab('show');
            this.summary.show(new NzbDrone.Episode.Summary.View({model: this.model}));

        },

        showActivity: function (e) {
            if (e) {
                e.preventDefault();
            }

            this.ui.activity.tab('show');
        },

        showSearch: function (e) {
            if (e) {
                e.preventDefault();
            }

            if (this._releaseSearchActivated) {
                return;
            }

            var self = this;

            this.ui.search.tab('show');
            this.search.show(new NzbDrone.Shared.SpinnerView());

            var releases = new NzbDrone.Release.Collection();
            var promise = releases.fetchEpisodeReleases(this.model.id);

            promise.done(function () {
                if (!self.isClosed) {
                    self.search.show(new NzbDrone.Episode.Search.Layout({collection: releases}));
                }
            });
        }

    });

});
