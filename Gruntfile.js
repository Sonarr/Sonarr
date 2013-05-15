module.exports = function (grunt) {

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        curl: {
            'UI/JsLibraries/backbone.js'                     : 'http://documentcloud.github.io/backbone/backbone.js',
            'UI/JsLibraries/backbone.marionette.js'          : 'http://marionettejs.com/downloads/backbone.marionette.js',
            'UI/JsLibraries/backbone.modelbinder.js'         : 'http://raw.github.com/theironcook/Backbone.ModelBinder/master/Backbone.ModelBinder.js',
            'UI/JsLibraries/backbone.mutators.js'            : 'http://raw.github.com/asciidisco/Backbone.Mutators/master/backbone.mutators.js',
            'UI/JsLibraries/backbone.shortcuts.js'           : 'http://raw.github.com/bry4n/backbone-shortcuts/master/backbone.shortcuts.js',
            'UI/JsLibraries/backbone.pageable.js'            : 'http://raw.github.com/wyuenho/backbone-pageable/master/lib/backbone-pageable.js',
            'UI/JsLibraries/backbone.backgrid.js'            : 'http://raw.github.com/wyuenho/backgrid/master/lib/backgrid.js',
            'UI/JsLibraries/backbone.backgrid.paginator.js'  : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/paginator/backgrid-paginator.js',
            'UI/JsLibraries/backbone.backgrid.filter.js'     : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/filter/backgrid-filter.js',

            'UI/JsLibraries/handlebars.runtime.js'           : 'http://raw.github.com/wycats/handlebars.js/master/dist/handlebars.runtime.js',

            'UI/JsLibraries/jquery.js'                       : 'http://code.jquery.com/jquery.js',
            'UI/JsLibraries/jquery.backstretch.js'           : 'http://raw.github.com/srobbin/jquery-backstretch/master/jquery.backstretch.js',
            'UI/JsLibraries/jquery.cookie.js'                : 'http://raw.github.com/carhartl/jquery-cookie/master/jquery.cookie.js',
            'UI/JsLibraries/jquery.signalR.js'               : 'https://raw.github.com/SignalR/SignalR/master/samples/Microsoft.AspNet.SignalR.Hosting.AspNet.Samples/Scripts/jquery.signalR.js',


            'UI/JsLibraries/require.js'                      : 'http://raw.github.com/jrburke/requirejs/master/require.js',
            'UI/JsLibraries/sugar.js'                        : 'http://raw.github.com/andrewplummer/Sugar/master/release/sugar-full.development.js',
            'UI/JsLibraries/underscore.js'                   : 'http://underscorejs.org/underscore.js',
            'UI/JsLibraries/lunr.js'                         : 'http://raw.github.com/olivernn/lunr.js/master/lunr.js',
         
            'UI/JsLibraries/messenger.js'                    : 'http://raw.github.com/HubSpot/messenger/master/build/js/messenger.js',
            'UI/Content/Messenger/messenger.css'             : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger.css',
            'UI/Content/Messenger/messenger.future.css'      : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger-theme-future.css',
            
            'UI/Content/bootstrap.toggle-switch.css'         : 'http://raw.github.com/ghinda/css-toggle-switch/gh-pages/toggle-switch.css',
        
            'UI/Content/FontAwesome/fontawesome.otf'         : 'http://github.com/FortAwesome/Font-Awesome/blob/master/build/assets/font-awesome/font/FontAwesome.otf?raw=true',
            'UI/Content/FontAwesome/fontawesome-webfont.eot' : 'http://github.com/FortAwesome/Font-Awesome/blob/master/build/assets/font-awesome/font/fontawesome-webfont.eot?raw=true',
            'UI/Content/FontAwesome/fontawesome-webfont.svg' : 'http://github.com/FortAwesome/Font-Awesome/blob/master/build/assets/font-awesome/font/fontawesome-webfont.svg?raw=true',
            'UI/Content/FontAwesome/fontawesome-webfont.ttf' : 'http://github.com/FortAwesome/Font-Awesome/blob/master/build/assets/font-awesome/font/fontawesome-webfont.ttf?raw=true',
            'UI/Content/FontAwesome/fontawesome-webfont.woff': 'http://github.com/FortAwesome/Font-Awesome/blob/master/build/assets/font-awesome/font/fontawesome-webfont.woff?raw=true',
            
            'UI/Content/FontAwesome/bootstrap.less'          : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/bootstrap.less',
            'UI/Content/FontAwesome/core.less'               : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/core.less',
            'UI/Content/FontAwesome/extras.less'             : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/extras.less',
            'UI/Content/FontAwesome/font-awesome-ie7.less'   : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/font-awesome-ie7.less',
            'UI/Content/FontAwesome/font-awesome.less'       : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/font-awesome.less',
            'UI/Content/FontAwesome/icons.less'              : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/icons.less',
            'UI/Content/FontAwesome/mixins.less'             : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/mixins.less',
            'UI/Content/FontAwesome/path.less'               : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/path.less',
            'UI/Content/FontAwesome/variables.less'          : 'http://raw.github.com/FortAwesome/Font-Awesome/master/build/assets/font-awesome/less/variables.less'
        },

        clean: {
            folder: "_output/UI/"
        },

        less  : {
            bootstrap: {
                src : "UI/Content/bootstrap/bootstrap.less",
                dest: "_output/UI/Content/bootstrap.css"
            },
            general  : {
                files: [
                    {
                        expand: true,
                        src   : ['UI/**/*.less', '!**/Bootstrap/**','!**/FontAwesome/**'],
                        dest  : '_output/',
                        ext   : '.css'
                    }
                ]
            }
        },

        handlebars: {
            options: {
                namespace  : "Templates",
                wrapped    : true,
                processName: function (fileName) {
                    return fileName
                        .replace('UI/', '')
                        .replace('.html', '')
                        .toLowerCase();
                }
            },
            files  : {
                src : ['UI/**/*emplate.html'],
                dest: '_output/UI/templates.js'
            }
        },

        copy: {
            index  : {
                src : 'UI/index.html',
                dest: '_output/UI/index.html'
            },
            scripts: {
                src : 'UI/**/*.js',
                dest: '_output/'
            },
            styles : {
                src : 'UI/**/*.css',
                dest: '_output/'
            },
            images : {
                src : 'UI/**/*.png',
                dest: '_output/'
            },
            fonts  : {
                src : 'UI/**/FontAwesome/*.*',
                dest: '_output/'
            }
        },

        watch: {
            bootstrap  : {
                files: ['<%= less.bootstrap.src %>', 'UI/**/FontAwesome/**'],
                tasks: ['less:bootstrap']
            },
            generalLess: {
                files: ['UI/**/*.less', '!**/Bootstrap/**', '!**/FontAwesome/**'],
                tasks: ['less:general']
            },
            handlebars : {
                files: '<%= handlebars.files.src %>',
                tasks: ['handlebars']
            },
            copyIndex  : {
                files: '<%= copy.index.src %>',
                tasks: ['copy:index']
            },
            copyScripts: {
                files: '<%= copy.scripts.src %>',
                tasks: ['copy:scripts']
            },
            copyStyles : {
                files: '<%= copy.styles.src %>',
                tasks: ['copy:styles']
            },
            copyImages : {
                files: '<%= copy.images.src %>',
                tasks: ['copy:images']
            },
            copyFonts  : {
                files: '<%= copy.fonts.src %>',
                tasks: ['copy:fonts']
            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-handlebars');
    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-notify');
    grunt.loadNpmTasks('grunt-curl');
    grunt.loadNpmTasks('grunt-clean');
    // Default task(s).
    grunt.registerTask('package', ['clean', 'copy', 'less', 'handlebars']);
    grunt.registerTask('default', ['package', 'watch']); 
    grunt.registerTask('update', ['curl']);

};