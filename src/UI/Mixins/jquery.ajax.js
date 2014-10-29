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
                xhr.headers['X-Api-Key'] = window.NzbDrone.ApiKey;
            }

            return original.apply(this, arguments).done(function (response, status, xhr){
                var version = xhr.getResponseHeader('X-ApplicationVersion');

                if (!window.NzbDrone || !window.NzbDrone.Version) {
                    return;
                }

                if (version !== window.NzbDrone.Version) {
                    var vent = require('vent');
                    var messenger = require('Shared/Messenger');

                    if (!vent || !messenger) {
                        return;
                    }

                    messenger.show({
                        message   : 'Sonarr has been updated',
                        hideAfter : 0,
                        id        : 'droneUpdated'
                    });

                    vent.trigger(vent.Events.ServerUpdated);
                }
            });
        };
    });
