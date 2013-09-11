'use strict';
define(
    [
        'app',
        'backbone',
        'Shared/Messenger',
        'Mixins/backbone.signalr.mixin'
    ], function (App, Backbone, Messenger) {

        var ProgressMessageCollection = Backbone.Collection.extend({
            url  : window.ApiRoot + '/progressmessage',
            model: Backbone.Model,

            initialize: function(){

            }

        });

        var collection = new ProgressMessageCollection();//.bindSignalR();

        /*        collection.signalRconnection.received(function (message) {

         var type = getMessengerType(message.status);
         var hideAfter = type === 'info' ? 60 :5;

         Messenger.show({
         id       : message.commandId,
         message  : message.message,
         type     : type,
         hideAfter: hideAfter
         });
         });*/

        var getMessengerType = function (status) {
            switch (status) {
                case 'completed':
                    return 'success';
                case 'failed':
                    return 'error';
                default:
                    return 'info';
            }
        };

        return collection;
    });
