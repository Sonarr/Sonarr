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
        src: ["NzbDrone.Backbone/Content/bootstrap/bootstrap.less"],
        dest: "_output/UI/Content/bootstrap.css"
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
          dest: '_output/UI/templates.js'
      },
    },

    copy:{
      index:{
        src: 'NzbDrone.Backbone/index.html', 
        dest: '_output/UI/index.html'
      },
      scripts:{
        expand:true,
        cwd: 'NzbDrone.Backbone/',
        src: '**/*.js', 
        dest: '_output/UI/'
      },
      styles:{
        expand:true,
        cwd: 'NzbDrone.Backbone/',
        src: '**/*.css', 
        dest: '_output/UI/'
      },
      images:{
        expand:true,
        cwd: 'NzbDrone.Backbone/',
        src: '**/*.png', 
        dest: '_output/UI/'
      },
      templates:{
        expand:true,
        cwd: 'NzbDrone.Backbone/',
        src: '**/*emplate.html', 
        dest: '_output/UI/'
      },
      fonts:{
        expand:true,
        src: 'NzbDrone.Backbone/Content/Fonts/*.*', 
        dest: '_output/UI/Content/Fonts/',
        flatten: true
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
        files: 'NzbDrone.Backbone/**/*.js',
        tasks: ['copy:scripts']  
      },
      copyStyles:{
        files: 'NzbDrone.Backbone/**/*.js',
        tasks: ['copy:styles']  
      },
      copyImages:{
        files: 'NzbDrone.Backbone/**/*.png',
        tasks: ['copy:images']  
      },
      copyTemplates:{
        files: '<%= handlebars.files.src %>',
        tasks: ['copy:templates']  
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