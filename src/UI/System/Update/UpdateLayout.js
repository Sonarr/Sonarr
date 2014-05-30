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
                this.backupCollection = new UpdateCollection();

                this.listenTo(this.backupCollection, 'sync', this._showUpdates);
            },

            onRender: function () {
                this.updates.show(new LoadingView());

                this.backupCollection.fetch();
            },

            _showUpdates: function () {
                this.updates.show(new UpdateCollectionView({ collection: this.backupCollection }));
            }
        });
    });
