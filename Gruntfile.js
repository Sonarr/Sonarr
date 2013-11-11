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

            'src/UI/JsLibraries/backbone.backgrid.selectall.js'  : 'http://raw.github.com/wyuenho/backgrid-select-all/master/backgrid-select-all.js',
            'src/UI/Content/Backgrid/selectall.css'              : 'http://raw.github.com/wyuenho/backgrid-select-all/master/backgrid-select-all.css',

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
            content: {
                cwd   : srcRoot,
                expand: true,
                src   : [
                            'index.html',
                            '**/*.css',
                            '**/*.png',
                            '**/*.jpg',
                            '**/*.ico',
                            '**/FontAwesome/*.*',
                            '**/fonts/*.*'
                        ],
                dest  : outputDir
            },
            scripts: {
                cwd   : srcRoot,
                expand: true,
                src   : [
                            '**/*.js',
                        ],
                dest  : outputDir
            }
        },

        jshint: {
            options: {
                 '-W030': false,
                 '-W064': false,
                 '-W097': false,
                 '-W100': false,
                 'undef': true,
                 'globals': {
                     'require': true,
                     'define': true,
                     'window': true,
                     'document': true,
                     'console': true
                 }
            },
            all: [
                srcRoot + '**/*.js',
                '!**/JsLibraries/*.js'
            ]
        },

        requirejs: {
            compile:{
                options: {
                    mainConfigFile: "src/UI/app.js",
                    fileExclusionRegExp: /^.*\.(?!js$)[^.]+$/,
                    preserveLicenseComments: false,
                    dir: outputDir,
                    optimize: 'none',
                    removeCombined: true,
                    inlineText: false,
                    keepBuildDir : true,
                    modules: [{
                        name: 'app',
                        exclude: ['JsLibraries/jquery', 'templates.js']
                    }],

                }
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
            copyContent  : {
                files: '<%= copy.content.cwd %><%= copy.content.src %>',
                tasks: ['copy:content']
            },
            copyScripts: {
                files: '<%= copy.scripts.cwd %><%= copy.scripts.src %>',
                tasks: ['copy:scripts']
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
    grunt.loadNpmTasks('grunt-contrib-requirejs');
    grunt.loadNpmTasks('grunt-contrib-jshint');

    grunt.registerTask('package', ['clean:output','handlebars', 'copy', 'less']);
    grunt.registerTask('packagerjs', ['clean:output','handlebars', 'requirejs', 'copy:content', 'less']);
    grunt.registerTask('default', ['package', 'watch']); 
    grunt.registerTask('update', ['curl']);

};
