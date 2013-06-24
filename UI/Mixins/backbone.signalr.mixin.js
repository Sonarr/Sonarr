'use strict';
define(
    [
        'signalR'
    ], function () {

        _.extend(Backbone.Collection.prototype, {BindSignalR: function (options) {

            if (!options || !options.url) {
                console.assert(this.url, 'url must be provided or collection must have url');
                options = {
                    url: this.url.replace('api', 'signalr')
                };
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


            var connection = $.connection(options.url);

            connection.stateChanged(function (change) {
                console.debug('{0} [{1}]'.format(options.url, _getStatus(change.newState)));
            });

            connection.received(function (model) {
                console.debug(model);
                self.fetch();
            });

            connection.start({ transport:
                [
                    'longPolling'
                ] });

            return this;
        }});
    });


