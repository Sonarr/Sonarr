var gulp = require('gulp');
var handlebars = require('gulp-handlebars');
var declare = require('gulp-declare');
var concat = require('gulp-concat');
var wrap = require("gulp-wrap");
var livereload = require('gulp-livereload');
var path = require('path');
var streamqueue = require('streamqueue');
var stripbom = require('gulp-stripbom');

var paths = require('./paths.js');

gulp.task('handlebars', function() {

    var coreStream = gulp.src([
        paths.src.templates,
        '!*/**/*Partial.*'
    ])
        .pipe(stripbom({ showLog : false }))
        .pipe(handlebars())
        .pipe(declare({
            namespace   : 'T',
            noRedeclare : true,
            processName : function(filePath) {

                filePath = path.relative(paths.src.root, filePath);

                return filePath.replace(/\\/g, '/')
                    .toLocaleLowerCase()
                    .replace('template', '')
                    .replace('.js', '');
            }
        }));

    var partialStream = gulp.src([paths.src.partials])
        .pipe(stripbom({ showLog : false }))
        .pipe(handlebars())
        .pipe(wrap('Handlebars.template(<%= contents %>)'))
        .pipe(wrap('Handlebars.registerPartial(<%= processPartialName(file.relative) %>, <%= contents %>)', {}, {
            imports : {
                processPartialName : function(fileName) {
                    return JSON.stringify(
                        path.basename(fileName, '.js')
                    );
                }
            }
        }));

    return streamqueue({ objectMode : true },
        partialStream,
        coreStream
    ).pipe(concat('templates.js'))
        .pipe(gulp.dest(paths.dest.root))
        .pipe(livereload());
});
