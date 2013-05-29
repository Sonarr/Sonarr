"use strict";
(function () {

    /*  var model = new NzbDrone.Shared.NotificationModel();
     model.set('title','test notification');
     model.set('message','test message');
     model.set('level', 'error');
     this.push(model);
     */


    if (!window.console) {
        window.console = {};
    }

    if (!window.console.debug) {
        window.console.debug = function () {

        };
    }

    window.alert = function (message) {
        window.Messenger().post(message);
    };

    window.onerror = function (msg, url, line) {

        try {

            var a = document.createElement('a');
            a.href = url;
            var messageText = a.pathname.split('/').pop() + ' : ' + line + "</br>" + msg;

            var message = {
                message        : messageText,
                type           : 'error',
                hideAfter      : 1000,
                showCloseButton: true
            };

            window.Messenger().post(message);

        } catch (error) {
            console.log("An error occurred while reporting error. " + error);
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
        } else {
            message.message = "[{0}] {1} : {2}".format(ajaxOptions.type, xmlHttpRequest.statusText, ajaxOptions.url);
        }

        window.Messenger().post(message);
        return false;
    });

})();

