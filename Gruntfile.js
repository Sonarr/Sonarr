module.exports = function(grunt) {

  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    uglify: {
      files: {
        expand: true, // Enable dynamic expansion.
        cwd: 'NzbDrone.Backbone/',      // Src matches are relative to this path.
        src: ['**/*.js'], // Actual pattern(s) to match.
        dest: 'build/',   // Destination path prefix.
        ext: '.min.js'   // Dest filepaths will have this extension.
      }
    },
    handlebars: {
      options: {
        namespace: "NzbDrone.Templates"
      },  
      files: {
        expand: true, // Enable dynamic expansion.
        cwd: 'NzbDrone.Backbone/',      // Src matches are relative to this path.
        src: ['**/*Template.html'],
        ext: '.hbs' // Actual pattern(s) to match.
      }
  }});

  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-handlebars');

  // Default task(s).
  grunt.registerTask('default', ['uglify']);

};