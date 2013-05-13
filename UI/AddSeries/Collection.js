"use strict";
define(['app', 'Series/SeriesModel'], function () {
    NzbDrone.AddSeries.Collection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series/lookup',
        model: NzbDrone.Series.SeriesModel,

        parse: function (response) {
            _.each(response, function (model) {
                model.id = undefined;
            });

            return response;
        }
    });
});
