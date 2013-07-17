'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers'
    ], function (Handlebars, FormatHelpers) {
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
            var currentTime = Date.create();
            var start = Date.create(this.airDate);
            var end = Date.create(this.end);

            if (currentTime.isBetween(start, end)) {
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
