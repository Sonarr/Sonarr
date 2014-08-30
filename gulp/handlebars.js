var gulp = require('gulp');
var handlebars = require('gulp-handlebars');
var declare = require('gulp-declare');
var concat = require('gulp-concat');
var wrapAmd = require('gulp-wrap-amd');
var wrap = require("gulp-wrap");
var path = require('path');
var streamqueue = require('streamqueue');

var paths = require('./paths.js');
var bom = require('./pipelines/gulp-bom.js');

gulp.task('handlebars', function () {

    var coreStream = gulp.src([paths.src.templates, '!*/**/*Partial.*'])
        .pipe(bom())
        .pipe(handlebars())
        .pipe(declare({
            namespace: 'T',
            noRedeclare: true,
            processName: function (filePath) {

                filePath = path.relative(paths.src.root, filePath);

                return filePath.replace(/\\/g, '/')
                    .toLocaleLowerCase()
                    .replace('template', '')
                    .replace('.js', '');
            }
        }));

    var partialStream = gulp.src([paths.src.partials])
        .pipe(bom())
        .pipe(handlebars())
        .pipe(wrap('Handlebars.template(<%= contents %>)'))
        .pipe(wrap('Handlebars.registerPartial(<%= processPartialName(file.relative) %>, <%= contents %>)', {}, {
            imports: {
                processPartialName: function (fileName) {
                    return JSON.stringify(
                        path.basename(fileName, '.js')
                    );
                }
            }
        }));


    return streamqueue({ objectMode: true },
        partialStream,
        coreStream
    ).pipe(concat('templates.js'))
        .pipe(wrapAmd({
            deps: ['handlebars'],
            params: ['Handlebars'],
            exports: 'this["T"]'
        }))
        .pipe(gulp.dest(paths.dest.root));
});
