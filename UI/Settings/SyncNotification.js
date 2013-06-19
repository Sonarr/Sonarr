"use strict";
define(['shared/messenger'], function (Messenger) {
    return {
        callback: function (options) {
            return {
                success: function () {
                    if (options.successMessage) {
                        Messenger.show({message: options.successMessage});
                    }

                    if (options.successCallback) {
                        options.successCallback.call(options.context);
                    }
                },
                error  : function () {
                    if (options.errorMessage) {
                        Messenger.show({message: options.errorMessage, type: 'error'});
                    }

                    if (options.errorCallback) {
                        options.errorCallback.call(options.context);
                    }
                }
            };
        }
    };
});
