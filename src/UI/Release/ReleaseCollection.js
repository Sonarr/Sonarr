﻿'use strict';
define(
    [
        'backbone.pageable',
        'Release/ReleaseModel',
        'Mixins/AsSortedCollection'
    ], function (PagableCollection, ReleaseModel, AsSortedCollection) {
        var Collection = PagableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/release',
            model: ReleaseModel,

            state: {
                pageSize : 2000,
                sortKey  : 'download',
                order    : -1
            },

            mode: 'client',
            
            sortMappings: {
                'quality'       : { sortKey: 'qualityWeight' },
                'rejections'    : { sortValue: function (model, attr) {
                                        var rejections = model.get('rejections');
                                        var releaseWeight = model.get('releaseWeight');

                                        if (rejections.length !== 0) {
                                            return releaseWeight + 1000000;
                                        }

                                        return releaseWeight;
                                    }
                },
                'download'      : { sortKey: 'releaseWeight' }
            },

            fetchEpisodeReleases: function (episodeId) {
                return this.fetch({ data: { episodeId: episodeId }});
            }
        });
        
        Collection = AsSortedCollection.call(Collection);
        
        return Collection;
    });
