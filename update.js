module.exports = function(grunt) {

	grunt.initConfig({
		pkg: grunt.file.readJSON('package.json'),
		curl: {
				'NzbDrone.Backbone/JsLibraries/jquery.js':						'http://code.jquery.com/jquery.js',
				'NzbDrone.Backbone/JsLibraries/backbone.collectionbinder.js': 	'http://raw.github.com/theironcook/Backbone.ModelBinder/master/Backbone.CollectionBinder.js',
				'NzbDrone.Backbone/JsLibraries/backbone.modelbinder.js': 		'http://raw.github.com/theironcook/Backbone.ModelBinder/master/Backbone.ModelBinder.js',
				'NzbDrone.Backbone/JsLibraries/backbone.js': 					'http://raw.github.com/documentcloud/backbone/master/backbone.js',
				'NzbDrone.Backbone/JsLibraries/backbone.marionette.js': 		'http://raw.github.com/marionettejs/backbone.marionette/master/lib/backbone.marionette.js',
				'NzbDrone.Backbone/JsLibraries/backbone.mutators.js': 			'http://raw.github.com/asciidisco/Backbone.Mutators/master/backbone.mutators.js',
			}

	});

	grunt.loadNpmTasks('grunt-curl');
  
	grunt.registerTask('default', ['curl']);
};