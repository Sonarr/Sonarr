'use strict';
define(
    [
        'marionette',
        'AddSeries/SearchResultView',
        'AddSeries/Collection'

    ], function (Marionette, SearchResultView, SearchResultCollection) {

        return Marionette.CollectionView.extend({

            itemView: SearchResultView,

            initialize: function (options) {


                this.isExisting = options.isExisting;
                this.fullResult = options.fullResult;

                this.listenTo(this.fullResult, 'sync', this._processResultCollection);
            },


            showAll: function () {

                this.showingAll = true;
                this.fullResult.each(function (searchResult) {
                    this.collection.add(searchResult);
                });

                this.render();
            },

            _processResultCollection: function () {
                if (!this.showingAll && this.isExisting) {
                    this.collection = new SearchResultCollection();
                    this.collection.add(this.fullResult.shift());
                }
                else {
                    this.collection = this.fullResult;
                }
            }


        });
    });
