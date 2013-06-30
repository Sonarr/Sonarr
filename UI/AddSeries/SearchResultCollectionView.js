'use strict';
define(
    [
        'marionette',
        'AddSeries/SearchResultView',

    ], function (Marionette, SearchResultView) {

        return Marionette.CollectionView.extend({

            itemView: SearchResultView,

            initialize: function (options) {

                this.isExisting = options.isExisting;
            },

            showAll: function () {
                this.showingAll = true;
                this.render();
            },

            appendHtml: function (collectionView, itemView, index) {
                if (!this.isExisting || this.showingAll || index === 0) {
                    collectionView.$el.append(itemView.el);
                }
            }

        });
    });
