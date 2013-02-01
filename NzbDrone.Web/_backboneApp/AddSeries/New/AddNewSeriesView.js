'use strict;'
/// <reference path="../../app.js" />
/// <reference path="SearchResultView.js" />

NzbDrone.AddSeries.AddNewSeriesView = Backbone.Marionette.Layout.extend({
    template: 'AddSeries/New/AddNewSeriesTemplate',
    route: 'Series/add/new',

    ui: {
        seriesSearch: '.search input'
    },

    regions: {
        searchResult: '#search-result',
    },

    collection: new NzbDrone.AddSeries.SearchResultCollection(),



    initialize: function (options) {
        if (options.rootFolders === undefined) {
            throw 'rootFolder arg. is required.';
        }

        if (options.qualityProfiles === undefined) {
            throw 'qualityProfiles arg. is required.';
        }


        this.rootFoldersCollection = options.rootFolders;
        this.qualityProfilesCollection = options.qualityProfiles;
    },

    onRender: function () {
        console.log('binding auto complete');
        var self = this;

        this.ui.seriesSearch
            .data('timeout', null)
            .keyup(function () {
                window.clearTimeout(self.$el.data('timeout'));
                self.$el.data('timeout', window.setTimeout(self.search, 500, self));
            });

        this.resultView = new NzbDrone.AddSeries.SearchResultView({ collection: this.collection });

    },

    search: function (context) {
        
        context.abortExistingRequest();
        
        var term = context.ui.seriesSearch.val();
        context.collection.reset();
        
        if (term !== '') {
            context.searchResult.show(new NzbDrone.Shared.SpinnerView());

            context.currentSearchRequest = context.collection.fetch({
                data: $.param({ term: term }),
                success: function (model) {
                    context.resultUpdated(model, context);
                }
            });

        } else {
            context.searchResult.close();
        }
    },

    abortExistingRequest : function () {
        if (this.currentSearchRequest && this.currentSearchRequest.readyState > 0 && this.currentSearchRequest.readyState < 4) {
            console.log('aborting previous pending search request.');
            this.currentSearchRequest.abort();
        }
    },


    resultUpdated: function (options, context) {
        _.each(options.models, function (model) {
            model.set('rootFolders', context.rootFoldersCollection);
            model.set('qualityProfiles', context.qualityProfilesCollection);
        });

        context.searchResult.show(context.resultView);
    }
});