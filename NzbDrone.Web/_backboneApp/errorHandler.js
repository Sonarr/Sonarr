/// <reference path="JsLibraries/jquery.js" />

window.onerror = function (msg, url, line) {
    alert("Error: " + msg + "\nurl: " + url + "\nline #: " + line);


    var suppressErrorAlert = true;
    // If you return true, then error alerts (like in older versions of 
    // Internet Explorer) will be suppressed.
    return suppressErrorAlert;
};

$(document).ajaxSuccess(function (event, XMLHttpRequest, ajaxOptionsa) {
    console.log(ajaxOptionsa);
});