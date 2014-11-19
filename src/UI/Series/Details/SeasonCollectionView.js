'use strict';
define(
    [
        'marionette',
        'Series/Details/SeasonLayout',
        'underscore'
    ], function (Marionette, SeasonLayout, _) {
        return Marionette.CollectionView.extend({

            itemView: SeasonLayout,

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection;
                this.series = options.series;
            },

            appendHtml: function(collectionView, itemView, index) {
                var childrenContainer = collectionView.itemViewContainer ? collectionView.$(collectionView.itemViewContainer) : collectionView.$el;
                var collection = collectionView.collection;

                // If the index of the model is at the end of the collection append, else insert at proper index
                if (index >= collection.size() - 1) {
                    childrenContainer.append(itemView.el);
                } else {
                    var previousModel = collection.at(index + 1);
                    var previousView = this.children.findByModel(previousModel);

                    if (previousView) {
                        previousView.$el.before(itemView.$el);
                    }

                    else {
                        childrenContainer.append(itemView.el);
                    }
                }
            },

            itemViewOptions: function () {
                return {
                    episodeCollection: this.episodeCollection,
                    series           : this.series
                };
            },

            onEpisodeGrabbed: function (message) {
                if (message.episode.series.id !== this.episodeCollection.seriesId) {
                    return;
                }

                var self = this;

                _.each(message.episode.episodes, function (episode) {
                    var ep = self.episodeCollection.get(episode.id);
                    ep.set('downloading', true);
                });

                this.render();
            }
        });
    });
