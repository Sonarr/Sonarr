"use strict";
define(['app', 'Episode/Summary/View'], function () {

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

            this.ui.search.tab('show');
        }

    });

});