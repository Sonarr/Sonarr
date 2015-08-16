var NzbDroneController = require('./Shared/NzbDroneController');
var AppLayout = require('./AppLayout');
var Marionette = require('marionette');
var ActivityLayout = require('./Activity/ActivityLayout');
var SettingsLayout = require('./Settings/SettingsLayout');
var AddSeriesLayout = require('./AddSeries/AddSeriesLayout');
var WantedLayout = require('./Wanted/WantedLayout');
var CalendarLayout = require('./Calendar/CalendarLayout');
var ReleaseLayout = require('./Release/ReleaseLayout');
var SystemLayout = require('./System/SystemLayout');
var SeasonPassLayout = require('./SeasonPass/SeasonPassLayout');
var SeriesEditorLayout = require('./Series/Editor/SeriesEditorLayout');
var AddMovieLayout = require('./AddMovie/AddMovieLayout');
var MovieLayout = require('./Movie/Index/MovieIndexLayout');
var MovieDetailsLayout = require('./Movie/Details/MovieDetailsLayout');
var MovieCollection = require('Movie/MovieCollection');

module.exports = NzbDroneController.extend({
    addSeries : function(action) {
        this.setTitle('Add Series');
        this.showMainRegion(new AddSeriesLayout({ action : action }));
    },

    calendar : function() {
        this.setTitle('Calendar');
        this.showMainRegion(new CalendarLayout());
    },

    settings : function(action) {
        this.setTitle('Settings');
        this.showMainRegion(new SettingsLayout({ action : action }));
    },

    wanted : function(action) {
        this.setTitle('Wanted');
        this.showMainRegion(new WantedLayout({ action : action }));
    },

    activity : function(action) {
        this.setTitle('Activity');
        this.showMainRegion(new ActivityLayout({ action : action }));
    },

    rss : function() {
        this.setTitle('RSS');
        this.showMainRegion(new ReleaseLayout());
    },

    system : function(action) {
        this.setTitle('System');
        this.showMainRegion(new SystemLayout({ action : action }));
    },

    seasonPass : function() {
        this.setTitle('Season Pass');
        this.showMainRegion(new SeasonPassLayout());
    },

    seriesEditor : function() {
        this.setTitle('Series Editor');
        this.showMainRegion(new SeriesEditorLayout());
    },
		
    addMovie : function(action) {
        this.setTitle('Add Movies');
        this.showMainRegion(new AddMovieLayout({ action : action }));	
    },
	
	movies : function(action) {
        this.setTitle('Movies');
        this.showMainRegion(new MovieLayout());
	},
	
	movieDetails : function(query) {
        var movie = MovieCollection.where({ cleanTitle : query });

        if (movie.length !== 0) {
            var targetMovie = movie[0];
            this.setTitle(targetMovie.get('title'));
            this.showMainRegion(new MovieDetailsLayout({ model : targetMovie }));
        } else {
            this.showNotFound();
        }
	}
});