var Backbone = require('backbone');
var WatchlistModel = require('./WatchlistModel');

module.exports = Backbone.Collection.extend({
	model : WatchlistModel,
	url   : window.NzbDrone.ApiRoot + '/watchlist'
});
