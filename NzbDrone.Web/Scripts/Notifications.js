/// <reference path="jquery-1.4.1-vsdoc.js" />
$(function () {
    alert("Notification");

    var container = $("#container-bottom").notify({ stack: 'above' });
    container.notify("create", {
        title: 'Look ma, two containers!',
        text: 'This container is positioned on the bottom of the screen.  Notifications will stack on top of each other with the <code>position</code> attribute set to <code>above</code>.'
    }, { expires: false });


});