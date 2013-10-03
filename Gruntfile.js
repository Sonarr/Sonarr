module.exports = function (grunt) {
    'use strict';  

    var outputRoot  = '_output/';
    var outputDir   =  outputRoot +'UI/';
    var srcRoot     = 'src/UI/'
    var srcContent  = srcRoot + 'Content/';
    var destContent = outputDir + 'Content/';

    grunt.initConfig({

        pkg: grunt.file.readJSON('package.json'),

        curl: {
            'src/UI/JsLibraries/backbone.js'                     : 'http://documentcloud.github.io/backbone/backbone.js',
            'src/UI/JsLibraries/backbone.marionette.js'          : 'http://marionettejs.com/downloads/backbone.marionette.js',
            'src/UI/JsLibraries/backbone.modelbinder.js'         : 'http://raw.github.com/theironcook/Backbone.ModelBinder/master/Backbone.ModelBinder.js',
            'src/UI/JsLibraries/backbone.shortcuts.js'           : 'http://raw.github.com/bry4n/backbone-shortcuts/master/backbone.shortcuts.js',

            'src/UI/JsLibraries/backbone.pageable.js'            : 'http://raw.github.com/wyuenho/backbone-pageable/master/lib/backbone-pageable.js',
            'src/UI/JsLibraries/backbone.backgrid.js'            : 'http://raw.github.com/wyuenho/backgrid/master/lib/backgrid.js',
            'src/UI/JsLibraries/backbone.backgrid.paginator.js'  : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/paginator/backgrid-paginator.js',
            'src/UI/JsLibraries/backbone.backgrid.filter.js'     : 'http://raw.github.com/wyuenho/backgrid/master/lib/extensions/filter/backgrid-filter.js',

            'src/UI/JsLibraries/backbone.validation.js'          : 'https://raw.github.com/thedersen/backbone.validation/master/dist/backbone-validation.js',

            'src/UI/JsLibraries/handlebars.runtime.js'           : 'http://raw.github.com/wycats/handlebars.js/master/dist/handlebars.runtime.js',
            'src/UI/JsLibraries/handlebars.helpers.js'           : 'http://raw.github.com/danharper/Handlebars-Helpers/master/helpers.js',

            'src/UI/JsLibraries/jquery.js'                       : 'http://code.jquery.com/jquery.js',
            'src/UI/JsLibraries/jquery.backstretch.js'           : 'http://raw.github.com/srobbin/jquery-backstretch/master/jquery.backstretch.js',
            'src/UI/JsLibraries/jquery.signalR.js'               : 'http://raw.github.com/SignalR/SignalR/master/samples/Microsoft.AspNet.SignalR.Hosting.AspNet.Samples/Scripts/jquery.signalR.js',
            'src/UI/JsLibraries/jquery.knob.js'                  : 'http://raw.github.com/aterrien/jQuery-Knob/master/js/jquery.knob.js',

            'src/UI/JsLibraries/require.js'                      : 'http://raw.github.com/jrburke/requirejs/master/require.js',
            'src/UI/JsLibraries/filesize.js'                     : 'http://cdn.filesizejs.com/filesize.js',
            'src/UI/JsLibraries/lodash.underscore.js'            : 'http://raw.github.com/bestiejs/lodash/master/dist/lodash.underscore.js',
         
            'src/UI/JsLibraries/messenger.js'                    : 'http://raw.github.com/HubSpot/messenger/master/build/js/messenger.js',
            'src/UI/Content/Messenger/messenger.css'             : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger.css',
            'src/UI/Content/Messenger/messenger.future.css'      : 'http://raw.github.com/HubSpot/messenger/master/build/css/messenger-theme-future.css',
            
            'src/UI/Content/bootstrap.toggle-switch.css'         : 'http://raw.github.com/ghinda/css-toggle-switch/gh-pages/toggle-switch.css',          
            'src/UI/Content/prefixer.less'                       : 'http://raw.github.com/JoelSutherland/LESS-Prefixer/master/prefixer.less'          
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
                cwd    : srcRoot,
                expand : true,
                src    :[
                            'Content/theme.less',
                            'Content/overrides.less',
                            'Series/series.less',
                            'AddSeries/addSeries.less',
                            'Calendar/calendar.less',
                            'Cells/cells.less',
                            'Settings/settings.less',
                            'System/Logs/logs.less',
                            'System/Update/update.less'
                        ],
                dest  : outputDir,
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
                        .replace(srcRoot, '')
                        .replace('.html', '')
                        .toLowerCase();
                }
            },
            files  : {
                src : [ srcRoot + '**/*Template.html', srcRoot + '**/*Partial.html'],
                dest: outputDir + 'templates.js'
            }
        },

        copy: {
            index  : {
                cwd   : srcRoot,
                expand: true,
                src   : '*ndex.html',
                dest  : outputDir
            },
            scripts: {
                cwd   : srcRoot,
                expand: true,
                src   : '**/*.js',
                dest  : outputDir
            },
            styles : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/*.css',
                dest  : outputDir
            },
            images : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/*.png',
                dest  : outputDir
            },
            jpg : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/*.jpg',
                dest  : outputDir
            },
            icon : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/*.ico',
                dest  : outputDir
            },
            fontAwesome  : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/FontAwesome/*.*',
                dest  : outputDir
            },
            fonts  : {
                cwd   : srcRoot,
                expand: true,
                src   : '**/fonts/*.*',
                dest  : outputDir
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
                files: [ srcRoot + '**/*.less', '!**/Bootstrap/**', '!**/FontAwesome/**'],
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
