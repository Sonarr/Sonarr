﻿'use strict';
define(
    [
        'backbone',
        'Commands/CommandModel',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, CommandModel) {

        var CommandCollection = Backbone.Collection.extend({
            url  : window.ApiRoot + '/command',
            model: CommandModel
        });

        var collection = new CommandCollection().bindSignalR();

        return collection;
    });
