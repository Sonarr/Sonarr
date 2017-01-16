var $ = require('jquery');
require('./AutoComplete');

$.fn.directoryAutoComplete = function(options) {
    options = options || {};

    var query = 'path';
    var data = {
        includeFiles: options.includeFiles || false
    };

    $(this).autoComplete({
        resource : '/filesystem',
        query    : query,
        data     : data,
        filter   : function(filter, response, callback) {
            var matches = [];
            var results = response.directories.concat(response.files);

            $.each(results, function(i, d) {
                if (d[query] && d[query].startsWith(filter)) {
                    matches.push({ value : d[query] });
                }
            });

            callback(matches);
        }
    });
};