module.exports = function(grunt) {

  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    uglify: {
        files: {
          expand: true, // Enable dynamic expansion.
          cwd: 'UI/',      // Src matches are relative to this path.
          src: ['**/*.js'], // Actual pattern(s) to match.
          dest: 'build/',   // Destination path prefix.
          ext: '.min.js'
        }
    },
    less:{
      bootstrap:{
        src: ["UI/Content/bootstrap/bootstrap.less"],
        dest: "_output/UI/Content/bootstrap.css"
      }
    },

    handlebars: {
      options: {
        namespace: "Templates",
        processName: function(fileName){
          return fileName
              .replace('UI/','')
              .replace('.html','');
        }
      },  
      files: {
          src: ['UI/**/*emplate.html'],
          dest: '_output/UI/templates.js'
      },
    },

    copy:{
      index:{
        src: 'UI/index.html', 
        dest: '_output/UI/index.html'
      },
      scripts:{
        src: 'UI/**/*.js', 
        dest: '_output/'
      },
      styles:{
        src: 'UI/**/*.css', 
        dest: '_output/'
      },
      images:{
        src: 'UI/**/*.png', 
        dest: '_output/'
      },
      fonts:{
        src: 'UI/**/Fonts/*.*', 
        dest: '_output/',
      }
    },

    watch:{
      bootstrap:{
        files: 'NzbDrone.Backbone/Content/bootstrap/*.less',
        tasks: ['less:bootstrap']  
      },
      handlebars:{
        files: '<%= handlebars.files.src %>',
        tasks: ['handlebars']  
      },
      copyIndex:{
        files: '<%= copy.index.src %>',
        tasks: ['copy:index']  
      },
      copyScripts:{
        files: '<%= copy.scripts.src %>',
        tasks: ['copy:scripts']  
      },
      copyStyles:{
        files: '<%= copy.styles.src %>',
        tasks: ['copy:styles']  
      },
      copyImages:{
        files: '<%= copy.images.src %>',
        tasks: ['copy:images']  
      },
      copyFonts:{
        files: '<%= copy.fonts.src %>',
        tasks: ['copy:fonts']  
      }
    }
  });

  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-handlebars');
  grunt.loadNpmTasks('grunt-contrib-less');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-wrap');

  // Default task(s).
  grunt.registerTask('default', ['copy','less:bootstrap','handlebars', 'watch']);

};