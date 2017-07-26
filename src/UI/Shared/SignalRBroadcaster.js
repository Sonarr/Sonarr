var vent = require('vent');
var $ = require('jquery');
var Messenger = require('./Messenger');
var StatusModel = require('../System/StatusModel');
require('signalR');

module.exports = {
    appInitializer : function() {
        console.log('starting signalR');

        var getStatus = function(status) {
            switch (status) {
                case 0:
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

        var tryingToReconnect = false;
        var messengerId = 'signalR';

        this.signalRconnection = $.connection(StatusModel.get('urlBase') + '/signalr', { apiKey: window.NzbDrone.ApiKey });

        this.signalRconnection.stateChanged(function(change) {
            console.debug('SignalR: [{0}]'.format(getStatus(change.newState)));
        });

        this.signalRconnection.received(function(message) {
            vent.trigger('server:' + message.name, message.body);
        });

        this.signalRconnection.reconnecting(function() {
            if (window.NzbDrone.unloading) {
                return;
            }

            tryingToReconnect = true;
        });

        this.signalRconnection.reconnected(function() {
            tryingToReconnect = false;
        });

        this.signalRconnection.disconnected(function() {
            if (tryingToReconnect) {
                $('<div class="modal-backdrop fade in"></div>').appendTo(document.body);

                Messenger.show({
                    id        : messengerId,
                    type      : 'error',
                    hideAfter : 0,
                    message   : 'Connection to backend lost',
                    actions   : {
                        cancel : {
                            label  : 'Reload',
                            action : function() {
                                window.location.reload();
                            }
                        }
                    }
                });
            }
        });

        this.signalRconnection.start({ transport : ['longPolling'] });

        return this;
    }
};
