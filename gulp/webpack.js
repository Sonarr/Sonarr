var gulp = require('gulp');
var gulpWebpack = require('webpack-stream');
var livereload = require('gulp-livereload');

var webpackConfig = require('../webpack.config');
webpackConfig.devtool = "#source-map";

gulp.task('webpack', function() {
    return gulp.src('main.js').pipe(gulpWebpack(webpackConfig)).pipe(gulp.dest(''));
});

gulp.task('webpackWatch', function() {
    webpackConfig.watch = true;
    return gulp.src('main.js').pipe(gulpWebpack(webpackConfig)).pipe(gulp.dest('')).pipe(livereload());
});
