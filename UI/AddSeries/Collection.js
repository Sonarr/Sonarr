"use strict";
define(['app', 'Series/SeriesModel'], function () {
    NzbDrone.AddSeries.Collection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series/lookup',
        model: NzbDrone.Series.SeriesModel,

        parse: function (response) {
            if (response) {
                response.id = undefined;
            }
            return response;
        }
    });
});
