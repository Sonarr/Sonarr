"use strict";
define([
    'app'
],
    function () {
        NzbDrone.Settings.SyncNotificaiton = {
            callback: function (options) {
                return {
                    success: function () {
                        if (options.successMessage) {
                            NzbDrone.Shared.Messenger.show({message: options.successMessage});
                        }

                        if (options.successCallback) {
                            options.successCallback.call(options.context);
                        }
                    },
                    error  : function () {
                        if (options.errorMessage) {
                            NzbDrone.Shared.Messenger.show({message: options.errorMessage, type: 'error'});
                        }

                        if (options.errorCallback) {
                            options.errorCallback.call(options.context);
                        }
                    }
                };
            }
        };
    });

