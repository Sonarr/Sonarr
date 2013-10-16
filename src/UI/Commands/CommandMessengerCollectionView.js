'use strict';
define(
    [
        'marionette',
        'Commands/CommandCollection',
        'Commands/CommandMessengerItemView'
    ], function (Marionette, commandCollection, CommandMessengerItemView) {

        var CollectionView = Marionette.CollectionView.extend({
            itemView: CommandMessengerItemView
        });

        return new CollectionView({collection: commandCollection});
    });
