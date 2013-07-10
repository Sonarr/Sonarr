'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers'
    ], function (Handlebars, FormatHelpers) {
        Handlebars.registerHelper('episodeNumberHelper', function () {

            if (this.series.seriesType === 'daily') {
                return FormatHelpers.DateHelper(this.airDate);
            }

            else {
                return '{0}x{1}'.format(this.seasonNumber, this.paddedEpisodeNumber);
            }

        });
    });
