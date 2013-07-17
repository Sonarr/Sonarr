﻿'use strict';
define(
    [
        'backbone',
        'moment',
        'Series/EpisodeModel'
    ], function (Backbone, Moment, EpisodeModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/calendar',
            model: EpisodeModel,

            comparator: function (model1, model2) {
                var airDate1 = model1.get('airDate');
                var date1 = Moment(airDate1);
                var time1 = date1.unix();

                var airDate2 = model2.get('airDate');
                var date2 = Moment(airDate2);
                var time2 = date2.unix();

                if (time1 < time2){
                    return -1;
                }

                if (time1 > time2){
                    return 1;
                }

                return 0;
            }
        });
    });
