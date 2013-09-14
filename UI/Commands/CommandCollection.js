'use strict';
define(
    [
        'backbone',
        'Commands/CommandModel',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, CommandModel) {

        var CommandCollection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/command',
            model: CommandModel,

            findCommand: function (command) {
                return this.find(function (model) {
                    return model.isSameCommand(command);
                });
            }

        });

        var collection = new CommandCollection().bindSignalR();

        collection.fetch();

        return collection;
    });
