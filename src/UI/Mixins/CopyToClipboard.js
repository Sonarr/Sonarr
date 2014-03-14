'use strict';

define([
    'jquery',
    'System/StatusModel',
    'zero.clipboard',
    'Shared/Messenger'
],
    function ($, StatusModel, ZeroClipboard, Messenger) {

    $.fn.copyToClipboard = function (input) {
        var moviePath = StatusModel.get('urlBase') + '/Content/zero.clipboard.swf';

        var client = new ZeroClipboard(this, {
            moviePath: moviePath
        });

        client.on('load', function(client) {
            client.on('dataRequested', function (client) {
                client.setText(input.val());
            });

            client.on('complete', function() {
                Messenger.show({
                    message: 'Copied text to clipboard'
                });
            } );
        } );
    };
});
