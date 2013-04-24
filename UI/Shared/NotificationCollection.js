"use strict";
define(['app', 'Shared/NotificationModel'], function () {

    var notificationCollection = Backbone.Collection.extend({

        model: NzbDrone.Shared.NotificationModel,

        initialize: function () {

            /*  var model = new NzbDrone.Shared.NotificationModel();
             model.set('title','test notification');
             model.set('message','test message');
             model.set('level', 'error');
             this.push(model);
             */

            window.alert = function (message) {
                window.Messenger().post(message);
            };


            var self = this;

            window.onerror = function (msg, url, line) {

                try {
                    var model = new NzbDrone.Shared.NotificationModel();

                    var a = document.createElement('a');
                    a.href = url;

                    model.set('title', a.pathname.split('/').pop() + ' : ' + line);
                    model.set('message', msg);
                    model.set('level', 'error');
                    self.push(model);
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

                var model = new NzbDrone.Shared.NotificationModel();
                model.set('level', 'error');

                if (xmlHttpRequest.status === 0 && xmlHttpRequest.readyState === 0) {
                    model.set('title', "Connection Failed");
                    model.set('message', "NzbDrone Server Not Reachable. make sure NzbDrone is running.");
                } else {
                    model.set('title', ajaxOptions.type + " " + ajaxOptions.url + " : " + xmlHttpRequest.statusText);
                    model.set('message', xmlHttpRequest.responseText);
                }

                self.push(model);
                return false;
            });
        }
    });

    return new notificationCollection();
});


