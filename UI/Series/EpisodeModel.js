"use strict";
define(['app'], function () {
    NzbDrone.Series.EpisodeModel = Backbone.Model.extend({

        mutators: {
            bestDateString     : function () {
                return bestDateString(this.get('airDate'));
            },
            paddedEpisodeNumber: function () {
                return this.get('episodeNumber');
            },
            allDay             : function () {
                return false;
            },
            day                : function () {
                return Date.create(this.get('airDate')).format('{dd}');
            },
            month              : function () {
                return Date.create(this.get('airDate')).format('{MON}');
            },
            startTime          : function () {
                var start = Date.create(this.get('airDate'));

                if (start.format('{mm}') === '00') {
                    return start.format('{h}{tt}');
                }

                return start.format('{h}.{mm}{tt}');
            },
            start              : function () {
                return this.get('airDate');
            },
            end              : function () {
                return this.get('endTime');
            },
            statusLevel        : function () {
                var status = this.get('status');
                var currentTime = Date.create();
                var start = Date.create(this.get('start'));
                var end = Date.create(this.get('end'));

                if (currentTime.isBetween(start, end)) {
                    return 'warning';
                }

                if (start.isBefore(currentTime) || status === 'Missing') {
                    return 'danger';
                }

                if (status === 'Ready') {
                    return 'success';
                }

                return 'primary';
            }
        },

        defaults: {
            seasonNumber: 0,
            status: 0
        }
    });
});
