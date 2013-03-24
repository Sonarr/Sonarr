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
          ext: '.min.js'
        }
    },
    less:{
      bootstrap:{
        src: ["NzbDrone.Backbone/Content/Bootstrap/bootstrap.less"],
        dest: "NzbDrone.Backbone/Content/bootstrap.css"
      }
    },

    handlebars: {
      options: {
        namespace: "NzbDrone.Templates",
        wrapped: false,
        processName: function(fileName){
          return fileName
              .replace('NzbDrone.Backbone/','')
              .replace('.html','');
        }
      },  
      files: {
          src: ['NzbDrone.Backbone/**/*emplate.html'],
          dest: 'NzbDrone.Backbone/templates.js'
      },
    },

    watch:{
      bootstrap:{
        files: '<%= less.bootstrap.src %>',
        tasks: ['less:bootstrap']  
      },
      handlebars:{
        files: '<%= handlebars.files.src %>',
        tasks: ['handlebars']  
      }

    }
  });

  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-handlebars');
  grunt.loadNpmTasks('grunt-contrib-less');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-contrib-concat');

  // Default task(s).
  grunt.registerTask('default', ['less:bootstrap','handlebars', 'watch']);

};