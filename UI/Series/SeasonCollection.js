'use strict';
define(['app', 'Series/SeasonModel', 'backbone.pageable'], function (App, SeasonModel, PageAbleCollection) {
    NzbDrone.Series.SeasonCollection = PageAbleCollection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/season',
        model: NzbDrone.Series.SeasonModel,

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


    return   NzbDrone.Series.SeasonCollection;
});
