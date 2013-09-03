﻿'use strict';
define(
    [
        'backbone',
        'ProgressMessaging/ProgressMessageModel',
        'Shared/Messenger',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, ProgressMessageModel, Messenger) {

        var ProgressMessageCollection = Backbone.Collection.extend({
            url  : window.ApiRoot + '/progressmessage',
            model: ProgressMessageModel
        });

        var collection = new ProgressMessageCollection().bindSignalR();

        collection.signalRconnection.received(function (message) {

            var type = getMessengerType(message.status);

            Messenger.show({
                id     : message.commandId,
                message: message.message,
                type   : type
            });
        });

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
