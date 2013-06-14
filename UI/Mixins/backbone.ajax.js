//try to add ajax data as query string to DELETE calls.
"use strict";
define(['jquery'], function () {

    var original = Backbone.ajax;

    Backbone.ajax = function () {

        var xhr = arguments[0];

        //check if ajax call was made with data option
        if (xhr && xhr.data && xhr.type === 'DELETE') {
            if (xhr.url.indexOf('?') === -1) {
                xhr.url = xhr.url + '?' + $.param(xhr.data);
            }
            else {
                xhr.url = xhr.url + '&' + $.param(xhr.data);
            }
        }

        return original.apply(this, arguments);
    };
});
