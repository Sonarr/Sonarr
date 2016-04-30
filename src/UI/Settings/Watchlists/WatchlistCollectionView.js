var Marionette = require('marionette');
var ItemView = require('./WatchlistItemView');
var SchemaModal = require('./Add/WatchlistSchemaModal');

module.exports = Marionette.CompositeView.extend({
	itemView          : ItemView,
	itemViewContainer : '.watchlist-list',
	template          : 'Settings/Notifications/NotificationCollectionViewTemplate',

	ui : {
		'addCard' : '.x-add-card'
	},

	events : {
		'click .x-add-card' : '_openSchemaModal'
	},

	appendHtml : function(collectionView, itemView, index) {
		collectionView.ui.addCare.parent('li').before(itemView.el);
	},

	_openSchemaModal : function() {
		SchemaModal.open(this.collection);
	}
});
