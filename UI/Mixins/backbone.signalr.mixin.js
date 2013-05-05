"use strict";
(function ($) {

    var connection = $.connection('/signalr/series');

    var _getStatus = function (status) {
        switch (status) {
            case    0:
                return 'connecting';
            case 1:
                return 'connected';
            case 2:
                return 'reconnecting';
            case 4:
                return 'disconnected';
            default:
                throw 'invalid status ' + status;
        }

    };

    connection.stateChanged(function (change) {

        console.log('signalR [{0}]'.format(_getStatus(change.newState)));
    });

    connection.received(function (data) {
        console.log(data);
    });

    connection.error(function (error) {
        console.warn(error);
    });

    connection.start();
})(jQuery);