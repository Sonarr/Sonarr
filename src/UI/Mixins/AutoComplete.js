'use strict';
define(
    [
        'jquery',
        'typeahead'
    ], function ($) {

    $.fn.autoComplete = function (resource) {
        $(this).typeahead({
                hint      : true,
                highlight : true,
                minLength : 3,
                items     : 20
            },
            {
                name: resource.replace('/'),
                displayKey: '',
                source   : function (filter, callback) {
                    $.ajax({
                        url     : window.NzbDrone.ApiRoot + resource,
                        dataType: 'json',
                        type    : 'GET',
                        data    : { query: filter },
                        success : function (data) {

                            var matches = [];

                            $.each(data, function(i, d) {
                                if (d.startsWith(filter)) {
                                    matches.push({ value: d });
                                }
                            });

                            callback(matches);
                        }
                    });
                }
        });
    };
});
