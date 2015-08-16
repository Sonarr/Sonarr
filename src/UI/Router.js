var Marionette = require('marionette');
var Controller = require('./Controller');

module.exports = Marionette.AppRouter.extend({
    controller : new Controller(),
    appRoutes  : {
        'addseries'                  : 'addSeries',
        'addseries/:action(/:query)' : 'addSeries',
        'calendar'                   : 'calendar',
        'settings'                   : 'settings',
        'settings/:action(/:query)'  : 'settings',
        'wanted'                     : 'wanted',
        'wanted/:action'             : 'wanted',
        'history'                    : 'activity',
        'history/:action'            : 'activity',
        'activity'                   : 'activity',
        'activity/:action'           : 'activity',
        'rss'                        : 'rss',
        'system'                     : 'system',
        'system/:action'             : 'system',
        'seasonpass'                 : 'seasonPass',
        'serieseditor'               : 'seriesEditor',
	    'addmovie'                   : 'addMovie',
	    'addmovie/:action(/:query)'  : 'addMovie',
		'movie'                      : 'movies',
		'movie/:action'              : 'movieDetails',
        ':whatever'                  : 'showNotFound'
    }
});