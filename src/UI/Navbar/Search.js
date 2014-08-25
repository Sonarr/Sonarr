'use strict';
define(
    [
        'underscore',
        'jquery',
        'vent',
        'backbone',
        'Series/SeriesCollection',
        'typeahead'
    ], function (_, $, vent, Backbone, SeriesCollection) {

        vent.on(vent.Hotkeys.NavbarSearch, function () {
            $('.x-series-search').focus();
        });

        $.fn.bindSearch = function () {
            $(this).typeahead({
                    hint: true,
                    highlight: true,
                    minLength: 1
                },
                {
                    name: 'series',
                    displayKey: 'title',
                    source: substringMatcher()
                });

            $(this).on('typeahead:selected typeahead:autocompleted', function (e, series) {
                this.blur();
                $(this).val('');
                Backbone.history.navigate('/series/{0}'.format(series.titleSlug), { trigger: true });
            });
        };

        var substringMatcher = function() {
            return function findMatches(q, cb) {
                // regex used to determine if a string contains the substring `q`
                var substrRegex = new RegExp(q, 'i');

                var matches = _.select(SeriesCollection.toJSON(), function (series) {
                    return substrRegex.test(series.title);
                });

                cb(matches);
            };
        };
    });
