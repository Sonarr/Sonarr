/// <reference path="jquery-1.5.2-vsdoc.js" />
$(function () {
    var speed = 0;
    refreshNotifications();

    var timer = window.setInterval(function () {
        speed = 500;
        refreshNotifications();
    }, 2000);

    function refreshNotifications() {
        $.ajax({
            url: '/Notification',
            success: notificationCallback
        });
    }

    function notificationCallback(data) {

        if (data === "") {
            CloseMsg();
        }
        else {
            DisplayMsg(data);
        }
    }

    //SetupNotifications();
    //DisplayMsg("Scanning Series Folder.");

    function DisplayMsg(sMsg) {
        //set the message text
        $("#msgText").text(sMsg);
        //show the message
        $('#msgBox').slideDown(speed, null);
    }

    function CloseMsg() {
        //hide the message
        $('#msgBox').slideUp(speed, null);
        //clear msg text
        $("#msgtText").val("");
    }
});


