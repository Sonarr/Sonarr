'use strict';
define(
    [
        'signalR'
    ], function () {

        _.extend(Backbone.Collection.prototype, {
            bindSignalR: function (options) {

                if (!options) {
                    options = {};
                }

                if (!options.url) {
                    console.assert(this.url, 'url must be provided or collection must have url');
                    options.url = this.url.replace('api', 'signalr');
                }

                var self = this;

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

                this.signalRconnection = $.connection(options.url);

                this.signalRconnection.stateChanged(function (change) {
                    console.debug('{0} [{1}]'.format(options.url, _getStatus(change.newState)));
                });

                this.signalRconnection.received(function (message) {
                    console.debug(message);
                    self.fetch();
                });

                this.signalRconnection.start({ transport:
                    [
                        'longPolling'
                    ] });

                return this;
            },

            unbindSignalR: function () {

                if(this.signalRconnection){
                    this.signalRconnection.stop();
                    delete this.signalRconnection;
                }

            }});
    });


