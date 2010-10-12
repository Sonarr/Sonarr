(function ($) {

    $(document).ready(function () {

        var popups = [];

        $.jGrowl.defaults.closer = false;
        $.jGrowl.defaults.closeTemplate = '';


        if (!$.browser.safari) {
            $.jGrowl.defaults.animateOpen = {
                width: 'show'
            };

            $.jGrowl.defaults.animateClose = {
                width: 'hide'
            };
        }
        refreshNotifications();
        var timer = window.setInterval(refreshNotifications, 2000);

        function refreshNotifications() {
            $.ajax({
                url: '/Notification',
                success: notificationCallback
            });
        }

        var failAjaxCounter = 0;
        function notificationCallback(data) {

            if (data === "") {
                failAjaxCounter = failAjaxCounter + 1;

                if (failAjaxCounter === 3) {
                    window.clearInterval(timer);
                }
            }
            else {
                failAjaxCounter = 0;
                for (var i in data) {

                    var titleId = data[i].Id + "_title";
                    var bodyId = data[i].Id + "_body";

                    //New Notification
                    if (popups[i] == undefined) {
                        popups[i] = new Object();
                        popups[i].Id = data;
                        $.jGrowl(MakeDiv(bodyId, data[i].CurrentStatus), { sticky: true, header: MakeDiv(titleId, data[i].Title), id: data[i].Id });
                    }
                    //Update Existing Notification
                    else {
                        $('#' + bodyId).html(data[i].CurrentStatus);
                    }
                }
            }
        }

        function cleanUp(data) {
        $(

        }

        function MakeDiv(id, body) {
            return "<div id ='" + id + "'>" + body + "</div>";
        }

    });
})(jQuery);
