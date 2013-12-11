'use strict';
define(
    [
        'underscore',
        'backbone',
        'Series/SeriesModel',
        'api!series'
    ], function (_, Backbone, SeriesModel, SeriesData) {
        var Collection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/series',
            model: SeriesModel,

            comparator: function (model) {
                return model.get('title');
            },

            state: {
                sortKey: 'title',
                order  : -1
            },

            save: function () {
                var self = this;

                var proxy = _.extend( new Backbone.Model(),
                    {
                        id: '',

                        url: self.url + '/editor',

                        toJSON: function()
                        {
                            return self.filter(function (model) {
                                return model.hasChanged();
                            });
                        }
                    });

                this.listenTo(proxy, 'sync', function (proxyModel, models) {
                    this.add(models, { merge: true });
                    this.trigger('save', this);
                });

                return proxy.save();
            }
        });

        var collection = new Collection(SeriesData);
        return collection;
    });
