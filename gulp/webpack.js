var gulp = require('gulp');

var gulpWebpack = require('gulp-webpack');
var webpack = require('webpack');
var webpackConfig = require('../webpack.config');

webpackConfig.devtool = "#source-map";

gulp.task('webpack', function() {
  return gulp.src('main.js')
    .pipe(gulpWebpack(webpackConfig, webpack))
    .pipe(gulp.dest(''));
});

gulp.task('webpackWatch', function() {
  webpackConfig.watch = true;
  return gulp.src('main.js')
    .pipe(gulpWebpack(webpackConfig, webpack))
    .pipe(gulp.dest(''));
});
