module.exports = function() {

    var $ = this;

    function modifyLocalRequest(xhr) {
        if (xhr && xhr.data && xhr.type === 'DELETE') {
            if (xhr.url.contains('?')) {
                xhr.url += '&';
            } else {
                xhr.url += '?';
            }
            xhr.url += $.param(xhr.data);
            delete xhr.data;
        }
        if (xhr) {
            xhr.headers = xhr.headers || {};
            xhr.headers['X-Api-Key'] = window.NzbDrone.ApiKey;
        }
    }

    var original = $.ajax;
    $.ajax = function(xhr) {
        'use strict';
        if (xhr.url.indexOf('/') === 0 || xhr.url.indexOf(window.location.origin) === 0) {
            modifyLocalRequest(xhr);
        }

        return original.apply(this, arguments);
    };
};
