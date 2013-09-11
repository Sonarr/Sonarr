'use strict';
define(
    [
        'Commands/CommandModel',
        'Commands/CommandCollection',
        'underscore'
    ], function (CommandModel, CommandCollection, _) {

        return{
            Execute: function (name, properties) {

                var attr = _.extend({name: name.toLocaleLowerCase()}, properties);

                var commandModel = new CommandModel(attr);

                return commandModel.save().success(function () {
                    CommandCollection.add(commandModel);
                });
            }
        }
    });
