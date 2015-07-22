var gulp = require('gulp');
var jshint = require('gulp-jshint');
var stylish = require('jshint-stylish');
var cache = require('gulp-cached');
var paths = require('./paths.js');

gulp.task('jshint', function() {
    return gulp.src([
        paths.src.scripts,
        paths.src.exclude.libs
    ])
        .pipe(cache('jshint'))
        .pipe(jshint())
        .pipe(jshint.reporter(stylish));
});
