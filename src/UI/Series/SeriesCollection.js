'use strict';
define(
    [
        'underscore',
        'backbone',
        'backbone.pageable',
        'Series/SeriesModel',
        'api!series',
        'Mixins/AsPersistedStateCollection',
        'moment'
    ], function (_, Backbone, PageableCollection, SeriesModel, SeriesData, AsPersistedStateCollection, Moment) {
        var Collection = PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/series',
            model: SeriesModel,
            tableName: 'series',

            state: {
                sortKey: 'title',
                order  : -1,
                pageSize: 1000
            },

            sorters: {
                nextAiring: function (model) {
                    var nextAiring = model.get('nextAiring');

                    if (!nextAiring) {
                        return Number.MAX_VALUE;
                    }

                    return Moment(nextAiring).unix();
                }
            },

            mode: 'client',

            save: function () {
                var self = this;

                var proxy = _.extend( new Backbone.Model(),
                    {
                        id: '',

                        url: self.url + '/editor',

                        toJSON: function()
                        {
                            return self.filter(function (model) {
                                return model.edited;
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

        var MixedIn = AsPersistedStateCollection.call(Collection);
        var collection = new MixedIn(SeriesData);
        collection.initialSort();

        return collection;
    });
