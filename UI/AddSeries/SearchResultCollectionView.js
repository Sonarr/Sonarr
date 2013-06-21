'use strict';
define(
    [
        'marionette',
        'AddSeries/SearchResultView'
    ], function (Marionette, SearchResultView) {

        return Marionette.CollectionView.extend({

            itemView  : SearchResultView,
            initialize: function () {
                this.listenTo(this.collection, 'reset', this.render);
            }

        });
    });
