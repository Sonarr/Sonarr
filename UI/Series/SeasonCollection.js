"use strict";
define(['app', 'Series/SeasonModel'], function () {
    NzbDrone.Series.SeasonCollection = Backbone.PageableCollection.extend({
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
});
