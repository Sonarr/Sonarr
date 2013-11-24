module.exports = function (grunt) {
    'use strict';  

    var outputRoot  = '_output/';
    var outputDir   =  outputRoot +'UI/';
    var srcRoot     = 'src/UI/';
    var srcContent  = srcRoot + 'Content/';
    var destContent = outputDir + 'Content/';

    grunt.initConfig({

        pkg: grunt.file.readJSON('package.json'),

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
                            '**/*.js'
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
                        exclude: ['templates.js']
                    }]
                }
            }
        },

        watch: {
            options: {
                nospawn: false
            },
            bootstrap  : {
                files: [ srcContent + 'Bootstrap/**', srcContent + 'FontAwesome/**'],
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
            content  : {
                files: [
                            srcRoot + '**/index.html',
                            srcRoot + '**/*.css',
                            srcRoot + '**/*.png',
                            srcRoot + '**/*.jpg',
                            srcRoot + '**/*.ico',
                            srcRoot + '**/FontAwesome/*.*',
                            srcRoot + '**/fonts/*.*'
                        ],
                tasks: ['copy:content']
            },
            scripts: {
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
    grunt.loadNpmTasks('grunt-contrib-requirejs');
    grunt.loadNpmTasks('grunt-contrib-jshint');

    grunt.registerTask('package', ['clean:output', 'jshint', 'handlebars', 'copy', 'less']);
    grunt.registerTask('packagerjs', ['clean:output','jshint', 'handlebars', 'requirejs', 'copy:content', 'less']);
    grunt.registerTask('default', ['package', 'watch']); 
};
