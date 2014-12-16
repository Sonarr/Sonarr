'use strict';
define(
    [
        'jquery',
        'Mixins/AutoComplete'
    ], function ($) {

    $.fn.directoryAutoComplete = function () {

        var query = 'path';

        $(this).autoComplete({
            resource : '/filesystem',
            query    : query,
            filter   : function (filter, response, callback) {

                var matches = [];

                $.each(response.directories, function(i, d) {
                    if (d[query] && d[query].startsWith(filter)) {
                        matches.push({ value: d[query] });
                    }
                });

                callback(matches);
            }
        });
    };
});
