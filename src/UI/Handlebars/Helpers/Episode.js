var Handlebars = require('handlebars');
var FormatHelpers = require('../../Shared/FormatHelpers');
var moment = require('moment');
require('../../Activity/Queue/QueueCollection');

Handlebars.registerHelper('EpisodeNumber', function() {

    if (this.series.seriesType === 'daily') {
        return moment(this.airDate).format('L');
    } else if (this.series.seriesType === 'anime' && this.absoluteEpisodeNumber !== undefined) {
        return '{0}x{1} ({2})'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2), FormatHelpers.pad(this.absoluteEpisodeNumber, 2));
    } else {
        return '{0}x{1}'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2));
    }
});

Handlebars.registerHelper('StatusLevel', function() {
    var hasFile = this.hasFile;
    var downloading = require('../../Activity/Queue/QueueCollection').findEpisode(this.id) || this.downloading;
    var currentTime = moment();
    var start = moment(this.airDateUtc);
    var end = moment(this.end);
    var monitored = this.series.monitored && this.monitored;

    if (hasFile) {
        return 'success';
    }

    if (downloading) {
        return 'purple';
    }

    else if (!monitored) {
        return 'unmonitored';
    }

    if (this.episodeNumber === 1) {
        return 'premiere';
    }

    if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
        return 'warning';
    }

    if (start.isBefore(currentTime) && !hasFile) {
        return 'danger';
    }

    return 'primary';
});

Handlebars.registerHelper('EpisodeProgressClass', function() {
    if (this.episodeFileCount === this.episodeCount) {
        if (this.status === 'continuing') {
            return '';
        }

        return 'progress-bar-success';
    }

    if (this.monitored) {
        return 'progress-bar-danger';
    }

    return 'progress-bar-warning';
});