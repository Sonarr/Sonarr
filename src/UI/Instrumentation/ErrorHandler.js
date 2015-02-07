var $ = require('jquery');
var Messenger = require('messenger');

window.alert = function(message) {
    new Messenger().post(message);
};

var addError = function(message) {
    $('#errors').append('<div>' + message + '</div>');
};

window.onerror = function(msg, url, line) {

    try {

        var a = document.createElement('a');
        a.href = url;
        var filename = a.pathname.split('/').pop();

        //Suppress Firefox debug errors when console window is closed
        if (filename.toLowerCase() === 'markupview.jsm' || filename.toLowerCase() === 'markup-view.js') {
            return false;
        }

        var messageText = filename + ' : ' + line + '</br>' + msg;

        var message = {
            message         : messageText,
            type            : 'error',
            hideAfter       : 1000,
            showCloseButton : true
        };

        new Messenger().post(message);

        addError(message.message);

    }
    catch (error) {
        console.log('An error occurred while reporting error. ' + error);
        console.log(msg);
        new Messenger().post('Couldn\'t report JS error.  ' + msg);
    }

    return false; //don't suppress default alerts and logs.
};

$(document).ajaxError(function(event, xmlHttpRequest, ajaxOptions) {

    //don't report 200 error codes
    if (xmlHttpRequest.status >= 200 && xmlHttpRequest.status <= 300) {
        return undefined;
    }

    //don't report aborted requests
    if (xmlHttpRequest.statusText === 'abort') {
        return undefined;
    }

    var message = {
        type            : 'error',
        hideAfter       : 1000,
        showCloseButton : true
    };

    if (xmlHttpRequest.status === 0 && xmlHttpRequest.readyState === 0) {
        return false;
    }

    if (xmlHttpRequest.status === 400 && ajaxOptions.isValidatedCall) {
        return false;
    }

    if (xmlHttpRequest.status === 503) {
        message.message = xmlHttpRequest.responseJSON.message;
    } else if (xmlHttpRequest.status === 409) {
        message.message = xmlHttpRequest.responseJSON.message;
    } else {
        message.message = '[{0}] {1} : {2}'.format(ajaxOptions.type, xmlHttpRequest.statusText, ajaxOptions.url);
    }

    new Messenger().post(message);
    addError(message.message);

    return false;
});