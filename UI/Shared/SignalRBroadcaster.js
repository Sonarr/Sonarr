'use strict';
define(
    [
        'app',
        'signalR'
    ], function () {
        return {

            appInitializer: function () {
                console.log('starting signalR');

                var getStatus = function (status) {
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

                this.signalRconnection = $.connection("/signalr");

                this.signalRconnection.stateChanged(function (change) {
                    console.debug('SignalR: [{0}]'.format(getStatus(change.newState)));
                });

                this.signalRconnection.received(function (message) {
                    require(
                        [
                            'app'
                        ], function (app) {
                            app.vent.trigger('server:' + message.name, message.body);
                        })
                });

                this.signalRconnection.start({ transport:
                    [
                        'longPolling'
                    ] });

                return this;
            }
        };
    });

