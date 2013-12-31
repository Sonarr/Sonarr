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

                this.listenTo(this.updateCollection, 'sync', this._showUpdates);
            },

            onRender: function () {
                this.updates.show(new LoadingView());

                this.updateCollection.fetch();
            },

            _showUpdates: function () {
                this.updates.show(new UpdateCollectionView({ collection: this.updateCollection }));
            }
        });
    });
