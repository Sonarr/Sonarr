'use strict';
(function () {

    window.console = window.console || {};
    window.console.log = window.console.log || function(){};
    window.console.group = window.console.group || function(){};
    window.console.groupEnd = window.console.groupEnd || function(){};
    window.console.debug = window.console.debug || function(){};
    window.console.warn = window.console.warn || function(){};
    window.console.assert = window.console.assert || function(){};

    window.alert = function (message) {
        window.Messenger().post(message);
    };

    window.onerror = function (msg, url, line) {

        try {

            var a = document.createElement('a');
            a.href = url;
            var filename = a.pathname.split('/').pop();

            //Suppress Firefox debug errors when console window is closed
            if (filename.toLowerCase() === 'markupview.jsm') {
                return false;
            }

            var messageText = filename + ' : ' + line + '</br>' + msg;

            var message = {
                message        : messageText,
                type           : 'error',
                hideAfter      : 1000,
                showCloseButton: true
            };

            window.Messenger().post(message);

        }
        catch (error) {
            console.log('An error occurred while reporting error. ' + error);
            console.log(msg);
            window.alert('Couldn\'t report JS error.  ' + msg);
        }

        return false; //don't suppress default alerts and logs.
    };

    $(document).ajaxError(function (event, xmlHttpRequest, ajaxOptions) {

        //don't report 200 error codes
        if (xmlHttpRequest.status >= 200 && xmlHttpRequest.status <= 300) {
            return undefined;
        }

        //don't report aborted requests
        if (xmlHttpRequest.statusText === 'abort') {
            return undefined;
        }

        var message = {
            type           : 'error',
            hideAfter      : 1000,
            showCloseButton: true
        };

        if (xmlHttpRequest.status === 0 && xmlHttpRequest.readyState === 0) {
            return false;
            //message.message = 'NzbDrone Server Not Reachable. make sure NzbDrone is running.';
        }
        else {
            message.message = '[{0}] {1} : {2}'.format(ajaxOptions.type, xmlHttpRequest.statusText, ajaxOptions.url);
        }

        window.Messenger().post(message);
        return false;
    });

})();

