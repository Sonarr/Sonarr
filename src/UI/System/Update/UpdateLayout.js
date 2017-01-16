var Marionette = require('marionette');
var Backgrid = require('backgrid');
var UpdateCollection = require('./UpdateCollection');
var UpdateCollectionView = require('./UpdateCollectionView');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'System/Update/UpdateLayoutTemplate',

    regions : {
        updates : '#x-updates'
    },

    initialize : function() {
        this.updateCollection = new UpdateCollection();

        this.listenTo(this.updateCollection, 'sync', this._showUpdates);
    },

    onRender : function() {
        this.updates.show(new LoadingView());

        this.updateCollection.fetch();
    },

    _showUpdates : function() {
        this.updates.show(new UpdateCollectionView({ collection : this.updateCollection }));
    }
});