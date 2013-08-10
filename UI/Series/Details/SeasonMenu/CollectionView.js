'use strict';
define(
    [
        'marionette',
        'Series/Details/SeasonMenu/ItemView'
    ], function (Marionette, ItemView) {
        return Marionette.CollectionView.extend({

            itemView: ItemView,

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection;
            },

            itemViewOptions: function () {
                return {
                    episodeCollection: this.episodeCollection
                };
            },

            appendHtml: function(collectionView, itemView){
                var childrenContainer = $(collectionView.childrenContainer || collectionView.el);
                childrenContainer.prepend(itemView.el);
            }
        });
    });
