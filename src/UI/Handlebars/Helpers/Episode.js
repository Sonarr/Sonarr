'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers',
        'moment'
    ], function (Handlebars, FormatHelpers, moment) {
        Handlebars.registerHelper('EpisodeNumber', function () {

            if (this.series.seriesType === 'daily') {
                return moment(this.airDate).format('L');
            }

            else {
                return '{0}x{1}'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2));
            }

        });

        Handlebars.registerHelper('StatusLevel', function () {

            var hasFile = this.hasFile;
            var downloading = require('History/Queue/QueueCollection').findEpisode(this.id) || this.downloading;
            var currentTime = moment();
            var start = moment(this.airDateUtc);
            var end = moment(this.end);

            if (hasFile) {
                return 'success';
            }

            if (downloading) {
                return 'purple';
            }

            if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
                return 'warning';
            }

            if (start.isBefore(currentTime) && !hasFile) {
                return 'danger';
            }

            return 'primary';

        });

        Handlebars.registerHelper('EpisodeProgressClass', function () {
            if (this.episodeFileCount === this.episodeCount) {
                if (this.status === 'continuing') {
                    return '';
                }

                return 'progress-bar-success';
            }

            return 'progress-bar-danger';
        });
    });
