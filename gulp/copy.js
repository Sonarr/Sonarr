var gulp = require('gulp');
var print = require('gulp-print');
var cache = require('gulp-cached');
var livereload = require('gulp-livereload');

var paths = require('./paths.js');

gulp.task('copyJs', function () {
  return gulp.src(
    [
      paths.src.root + 'polyfills.js',
      paths.src.root + 'JsLibraries/handlebars.runtime.js'
    ])
    .pipe(cache('copyJs'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyHtml', function () {
  return gulp.src(paths.src.html)
    .pipe(cache('copyHtml'))
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyContent', function () {
  return gulp.src([paths.src.content + '**/*.*', '!**/*.less'])
    .pipe(gulp.dest(paths.dest.content))
    .pipe(livereload());
});
