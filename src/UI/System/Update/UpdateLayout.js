'use strict';
define(
    [
        'marionette',
        'backgrid',
        'System/Update/UpdateCollection',
        'System/Update/UpdateCollectionView',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, UpdateCollection, UpdateCollectionView, LoadingView) {
        return Marionette.Layout.extend({
            template: 'System/Update/UpdateLayoutTemplate',

            regions: {
                updates: '#x-updates'
            },

            initialize: function () {
                this.updateCollection = new UpdateCollection();
            },

            onRender: function () {
                this.updates.show(new LoadingView());

                var self = this;
                var promise = this.updateCollection.fetch();

                promise.done(function (){
                    self.updates.show(new UpdateCollectionView({ collection: self.updateCollection }));
                });
            }
        });
    });
