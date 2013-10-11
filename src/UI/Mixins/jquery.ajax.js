//try to add ajax data as query string to DELETE calls.
'use strict';
define(
    [
        'jquery'
    ], function ($) {

        var original = $.ajax;

        $.ajax = function (xhr) {

            if (xhr && xhr.data && xhr.type === 'DELETE') {

                if (xhr.url.contains('?')) {
                    xhr.url += '&';
                }
                else {
                    xhr.url += '?';
                }

                xhr.url += $.param(xhr.data);

                delete xhr.data;
            }

            if (xhr) {
                xhr.headers = xhr.headers || {};
                xhr.headers.Authorization = window.NzbDrone.ApiKey;
            }

            return original.apply(this, arguments);
        };
    });
