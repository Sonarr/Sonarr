'use strict';
define(
    [
        'handlebars',
        'underscore'
    ], function (Handlebars, _) {
        Handlebars.registerHelper('poster', function () {

            var poster = _.where(this.images, {coverType: 'poster'});

            if (poster[0]) {
                return poster[0].url;
            }

            return undefined;
        });

        Handlebars.registerHelper('traktUrl', function () {
            return 'http://trakt.tv/show/' + this.titleSlug;
        });

        Handlebars.registerHelper('imdbUrl', function () {
            return 'http://imdb.com/title/' + this.imdbId;
        });

        Handlebars.registerHelper('tvdbUrl', function () {
            return 'http://www.thetvdb.com/?tab=series&id=' + this.tvdbId;
        });

        Handlebars.registerHelper('tvRageUrl', function () {
            return 'http://www.tvrage.com/shows/id-' + this.tvRageId;
        });

        Handlebars.registerHelper('route', function () {
            return '/series/' + this.titleSlug;
        });


        Handlebars.registerHelper('percentOfEpisodes', function () {
            var episodeCount = this.episodeCount;
            var episodeFileCount = this.episodeFileCount;

            var percent = 100;

            if (episodeCount > 0) {
                percent = episodeFileCount / episodeCount * 100;
            }

            return percent;
        });

    });
