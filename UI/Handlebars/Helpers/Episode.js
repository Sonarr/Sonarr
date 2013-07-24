'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers',
        'moment'
    ], function (Handlebars, FormatHelpers, Moment) {
        Handlebars.registerHelper('EpisodeNumber', function () {

            if (this.series.seriesType === 'daily') {
                return Moment(this.airDateUtc).format('L');
            }

            else {
                return '{0}x{1}'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2));
            }

        });

        Handlebars.registerHelper('StatusLevel', function () {

            var hasFile = this.hasFile;
            var currentTime = Moment();
            var start = Moment(this.airDateUtc);
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
