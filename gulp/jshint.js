var gulp = require('gulp');
var jshint = require('gulp-jshint');
var stylish = require('jshint-stylish');
var cache = require('gulp-cached');
var paths = require('./paths.js');


gulp.task('jshint', function () {
    return gulp.src([paths.src.scripts, paths.src.exclude.libs])
        .pipe(cache('jshint'))
        .pipe(jshint({
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
        }))
        .pipe(jshint.reporter(stylish));
});