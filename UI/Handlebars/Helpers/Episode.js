'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers',
        'moment'
    ], function (Handlebars, FormatHelpers, Moment) {
        Handlebars.registerHelper('EpisodeNumber', function () {

            if (this.series.seriesType === 'daily') {
                return FormatHelpers.DateHelper(this.airDate);
            }

            else {
                return '{0}x{1}'.format(this.seasonNumber, this.episodeNumber.pad(2));
            }

        });

        Handlebars.registerHelper('StatusLevel', function () {

            var hasFile = this.hasFile;
            var currentTime = Moment();
            var start = Moment(this.airDate);
            var end = Moment(this.end);

            if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
                return 'warning';
            }

            if (start.isBefore(currentTime) && !hasFile) {
                return 'danger';
            }

            if (hasFile) {
                return 'success';
            }

            return 'primary';

        });
    });
