var gulp = require('gulp');
var print = require('gulp-print');
var cache = require('gulp-cached');

var paths = require('./paths.js');

gulp.task('copyJs', function () {
    return gulp.src(paths.src.scripts)
        .pipe(cache('copyJs'))
        .pipe(print())
        .pipe(gulp.dest(paths.dest.root));
});

gulp.task('copyIndex', function () {
    return gulp.src(paths.src.index)
        .pipe(cache('copyIndex'))
        .pipe(gulp.dest(paths.dest.root));
});

gulp.task('copyContent', function () {
    return gulp.src([paths.src.content + '**/*.*', '!**/*.less'])
        .pipe(gulp.dest(paths.dest.content));
});