'use strict';
define(function () {
    return {
        show: function (options) {

            if (!options.type) {
                options.type = 'info';
            }

            if (!options.hideAfter) {
                switch (options.type) {
                    case 'info':
                        options.hideAfter = 5;
                        break;

                    default :
                        options.hideAfter = 0;
                }
            }

            return  window.Messenger().post({
                message        : options.message,
                type           : options.type,
                showCloseButton: true,
                hideAfter      : options.hideAfter
            });
        },


        monitor: function (options) {

            if (!options.promise) {
                throw 'promise is required';
            }

            if (!options.successMessage) {
                throw 'success message is required';
            }

            if (!options.errorMessage) {
                throw 'error message is required';
            }

            var self = this;

            options.promise.done(function () {
                self.show({message: options.successMessage});
            });

            options.promise.fail(function () {
                self.show({message: options.errorMessage, type: 'error'});
            });

            return options.promise;
        }
    };
});

