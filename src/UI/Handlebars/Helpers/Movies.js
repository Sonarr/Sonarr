var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');
var _ = require('underscore');

Handlebars.registerHelper('moviePoster', function() {

    var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
    var poster = _.where(this.images, { coverType : 'poster' });

    if (poster[0]) {
        if (!poster[0].url.match(/^https?:\/\//)) {
            return new Handlebars.SafeString('<img class="movie-poster x-movie-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, poster[0].url, 250)));
        } else {
            var url = poster[0].url.replace(/^https?\:/, '');
            return new Handlebars.SafeString('<img class="movie-poster x-movie-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, url)));
        }
    }

    return new Handlebars.SafeString('<img class="movie-poster placeholder-image" src="{0}">'.format(placeholder));
});

Handlebars.registerHelper('movieRoute', function() {
    return StatusModel.get('urlBase') + '/movie/' + this.cleanTitle;
});

Handlebars.registerHelper('originalTitleWithYear', function() {
    if (this.originalTitle.endsWith(' ({0})'.format(this.year))) {
        return this.originalTitle;
    }

    if (!this.year) {
        return this.originalTitle;
    }

    return new Handlebars.SafeString('{0} <span class="year">({1})</span>'.format(this.originalTitle, this.year));
});

Handlebars.registerHelper('MovieProgressClass', function() {
    var file = this.movieFileId !== 0;

    if (file) {
        return 'progress-bar-success';
    }

    if (this.monitored) {
        return 'progress-bar-danger';
    }

    return 'progress-bar-warning';
});