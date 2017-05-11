var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Backbone = require('backbone');
var jdu = require('jdu');
var SeriesCollection = require('../Series/SeriesCollection');
require('typeahead');


vent.on(vent.Hotkeys.NavbarSearch, function() {
    $('.x-series-search').focus();
});

var stringCleaner = function(text) {
    return jdu.replace(text.toLowerCase());
};

var substringMatcher = function() {
    return function findMatches (q, cb) {
        var matches = _.select(SeriesCollection.toJSON(), function(series) {
            return stringCleaner(series.title).indexOf(stringCleaner(q)) > -1;
        });
        cb(matches);
    };
};

$.fn.bindSearch = function() {
    $(this).typeahead({
        hint      : true,
        minLength : 1
    }, {
        name       : 'series',
        displayKey : 'title',
        source     : substringMatcher()
    });

    $(this).on('typeahead:selected typeahead:autocompleted', function(e, series) {
        this.blur();
        $(this).val('');
        Backbone.history.navigate('/series/{0}'.format(series.titleSlug), { trigger : true });
    });
};
