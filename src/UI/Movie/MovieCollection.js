var _ = require('underscore');
var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var MovieModel = require('./MovieModel');
var AsFilteredCollection = require('../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../Mixins/AsPersistedStateCollection');
var ApiData = require('../Shared/ApiData');

var Collection = PageableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/movies',
    model     : MovieModel,
    tableName : 'movies',

    state : {
        sortKey            : 'sortTitle',
        order              : -1,
        pageSize           : 100000,
        secondarySortKey   : 'sortTitle',
        secondarySortOrder : -1
    },

    mode : 'client',

    save : function() {
        var self = this;

        var proxy = _.extend(new Backbone.Model(), {
            id : '',

            url : self.url + '/editor',

            toJSON : function() {
                return self.filter(function(model) {
                    return model.edited;
                });
            }
        });

        this.listenTo(proxy, 'sync', function(proxyModel, models) {
            this.add(models, { merge : true });
            this.trigger('save', this);
        });

        return proxy.save();
    },

    sortMappings : {
        title : {
            sortKey : 'title'
        },

		path : {
			sortValue : function(model) {
				var path = model.get('path');

				return path.toLowerCase();
			}
		}
    }
});
	
Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

var data = ApiData.get('movies');

module.exports = new Collection(data, { full : true });	
/*var MovieCollection = Backbone.Collection.extend({
    model : MovieModel,
    url   : window.NzbDrone.ApiRoot + '/movies'
});

var movies = new MovieCollection();

movies.fetch();

module.exports = movies;*/
