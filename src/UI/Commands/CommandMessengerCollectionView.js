'use strict';
define(
    [
        'app',
        'marionette',
        'Commands/CommandCollection',
        'Commands/CommandMessengerItemView'
    ], function (App, Marionette, commandCollection, CommandMessengerItemView) {

        var CollectionView = Marionette.CollectionView.extend({
            itemView : CommandMessengerItemView
        });

        new CollectionView({collection: commandCollection});
    });
