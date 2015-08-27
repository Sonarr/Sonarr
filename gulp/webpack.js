var gulp = require('gulp');
var webpackStream = require('webpack-stream');
var livereload = require('gulp-livereload');
var webpackConfig = require('../webpack.config');

gulp.task('webpack', function() {
    return gulp.src('main.js').pipe(webpackStream(webpackConfig)).pipe(gulp.dest(''));
});

gulp.task('webpackWatch', function() {
    webpackConfig.watch = true;
    return gulp.src('main.js').pipe(webpackStream(webpackConfig)).pipe(gulp.dest('')).pipe(livereload());
});
