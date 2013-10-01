module.exports = function (grunt) {
    'use strict';  

    var outputRoot = '_output/';
    var outputDir =  outputRoot +'UI/';
    var srcContent = 'UI/Content/';
    var destContent = outputDir + 'Content/';

    grunt.initConfig({

        pkg: grunt.file.readJSON('package.json'),

        curl: {
            'UI/JsLibraries/backbone.js'                     : 'http://documentcloud.github.io/backbone/backbone.js',
            'UI/JsLibraries/backbone.marionette.js'          : 'http://marionettejs.com/downloads/backbone.marionette.js',
            'UI/JsLibraries/backbone.modelbinder.js'         : 'http://raw.github.com/theironcook/Backbone.ModelBinder/master/Backbone.ModelBinder.js',
            'UI/JsLibraries/backbone.shortcuts.js'           : 'http://raw.github.com/bry4n/backbone-shortcuts/master/backbone.shortcuts.js',

            'UI/JsLibraries/backbone.pageable.js'            : 'http://raw.github.com/wyuenho/backbone-pageable/master/lib/backbone-pageable.js',
            'UI/JsLibraries/backbone.backgrid.js'            : 'http://raw.github.com/wyuenho/backgrid/master/lib/backgrid.js',
            'UI/JsLibraries/backbone.backgrid.paginator.js'  : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/paginator/backgrid-paginator.js',
            'UI/JsLibraries/backbone.backgrid.filter.js'     : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/filter/backgrid-filter.js',

            'UI/JsLibraries/backbone.validation.js'          : 'https://raw.github.com/thedersen/backbone.validation/master/dist/backbone-validation.js',

            'UI/JsLibraries/handlebars.runtime.js'           : 'http://raw.github.com/wycats/handlebars.js/master/dist/handlebars.runtime.js',
            'UI/JsLibraries/handlebars.helpers.js'           : 'http://raw.github.com/danharper/Handlebars-Helpers/master/helpers.js',

            'UI/JsLibraries/jquery.js'                       : 'http://code.jquery.com/jquery.js',
            'UI/JsLibraries/jquery.backstretch.js'           : 'http://raw.github.com/srobbin/jquery-backstretch/master/jquery.backstretch.js',
            'UI/JsLibraries/jquery.signalR.js'               : 'http://raw.github.com/SignalR/SignalR/master/samples/Microsoft.AspNet.SignalR.Hosting.AspNet.Samples/Scripts/jquery.signalR.js',
            'UI/JsLibraries/jquery.knob.js'                  : 'http://raw.github.com/aterrien/jQuery-Knob/master/js/jquery.knob.js',

            'UI/JsLibraries/require.js'                      : 'http://raw.github.com/jrburke/requirejs/master/require.js',
            'UI/JsLibraries/filesize.js'                     : 'http://cdn.filesizejs.com/filesize.js',
            'UI/JsLibraries/lodash.underscore.js'            : 'http://raw.github.com/bestiejs/lodash/master/dist/lodash.underscore.js',
         
            'UI/JsLibraries/messenger.js'                    : 'http://raw.github.com/HubSpot/messenger/master/build/js/messenger.js',
            'UI/Content/Messenger/messenger.css'             : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger.css',
            'UI/Content/Messenger/messenger.future.css'      : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger-theme-future.css',
            
            'UI/Content/bootstrap.toggle-switch.css'         : 'http://raw.github.com/ghinda/css-toggle-switch/gh-pages/toggle-switch.css',          
            'UI/Content/prefixer.less'                       : 'http://raw.github.com/JoelSutherland/LESS-Prefixer/master/prefixer.less'          
        },

        clean: {
            output:  outputDir,
            scripts: [ outputDir + '/**.js','!_output/UI/**/templates.js']
        },

        less  : {

            options:{
                dumpLineNumbers : 'false',
                compress        : true,
                yuicompress     : false,
                ieCompat        : true,
                strictImports   : true
            },

            bootstrap: {
                src : srcContent + 'Bootstrap/bootstrap.less',
                dest: destContent + 'bootstrap.css'
            },
            general  : {
                expand :true,
                src   : [
                            'UI/Content/theme.less',
                            'UI/Content/overrides.less',
                            'UI/Series/series.less',
                            'UI/AddSeries/addSeries.less',
                            'UI/Calendar/calendar.less',
                            'UI/Cells/cells.less',
                            'UI/Settings/settings.less',
                            'UI/System/Logs/logs.less',
                            'UI/System/Update/update.less'
                        ],
                dest  : outputRoot,
                ext: '.css'
            }
        },

        handlebars: {
            options: {
                namespace   : 'T',
                partialRegex: /Partial.html/,
                wrapped     : true,
                amd         : true,
                processName: function (fileName) {
                    return fileName
                        .replace('UI/', '')
                        .replace('.html', '')
                        .toLowerCase();
                }
            },
            files  : {
                src : ['UI/**/*Template.html','UI/**/*Partial.html'],
                dest: outputDir + 'templates.js'
            }
        },

        copy: {
            index  : {
                src : 'UI/*ndex.html',
                dest: outputRoot
            },
            scripts: {
                src : 'UI/**/*.js',
                dest: outputRoot
            },
            styles : {
                src : 'UI/**/*.css',
                dest: outputRoot
            },
            images : {
                src : 'UI/**/*.png',
                dest: outputRoot
            },
            jpg : {
                src : 'UI/**/*.jpg',
                dest: outputRoot
            },
            icon : {
                src : 'UI/**/*.ico',
                dest: outputRoot
            },
            fontAwesome  : {
                src : 'UI/**/FontAwesome/*.*',
                dest: outputRoot
            },
            fonts  : {
                src : 'UI/**/fonts/*.*',
                dest: outputRoot
            }
        },

        watch: {
            options: {
                nospawn: false,
            },
            bootstrap  : {
                files: [ srcContent + 'Bootstrap/**', srcContent +'FontAwesome/**'],
                tasks: ['less:bootstrap','less:general']
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
            copyJpg : {
                files: '<%= copy.jpg.src %>',
                tasks: ['copy:jpg']
            },
            copyIcon : {
                files: '<%= copy.icon.src %>',
                tasks: ['copy:icon']
            },
            copyFontAwesome  : {
                files: '<%= copy.fontAwesome.src %>',
                tasks: ['copy:fontAwesome']
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
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-notify');
    grunt.loadNpmTasks('grunt-curl');

    grunt.registerTask('package', ['clean:output', 'copy', 'less', 'handlebars']);
    grunt.registerTask('default', ['package', 'watch']); 
    grunt.registerTask('update', ['curl']);

};
