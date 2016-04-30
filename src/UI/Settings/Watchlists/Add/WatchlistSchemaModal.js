var AppLayout = require('../../../AppLayout');
var SchemaCollection = require('../WatchlistCollection');
var AddCollectionView = require('./WatchlistAddCollectionView');

module.exports = {
	open : function(collection) {
		var schemaCollection = new SchemaCollection();
		var originalUrl = schemaCollection.url;
		schemaCollection.url = schemaCollection.url + '/schema';
		schemaCollection.fetch();
		schemaCollection.url = originalUrl;
		var view = new AddCollectionView({
			collection       : schemaCollection,
			targetCollection : collection
		});
		AppLayout.modalRegion.show(view);
	}
};
