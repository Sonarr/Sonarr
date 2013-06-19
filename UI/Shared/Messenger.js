"use strict";
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
                    case 'error':
                        options.hideAfter = 0;
                }
            }

            return  window.Messenger().post({
                message        : options.message,
                type           : options.type,
                showCloseButton: true,
                hideAfter      : options.hideAfter
            });
        }};
});
