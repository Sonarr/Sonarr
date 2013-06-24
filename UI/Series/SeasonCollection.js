﻿'use strict';
define(
    [
        'Series/SeasonModel',
        'backbone.pageable'
    ], function (SeasonModel, PageAbleCollection) {
        return PageAbleCollection.extend({
            url  : window.ApiRoot + '/season',
            model: SeasonModel,

            mode: 'client',

            state: {
                sortKey : 'seasonNumber',
                order   : 1,
                pageSize: 1000000
            },

            queryParams: {
                sortKey: null,
                order  : null
            }
        });
    });
