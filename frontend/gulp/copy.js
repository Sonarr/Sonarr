var path = require('path');
var gulp = require('gulp');
var print = require('gulp-print');
var cache = require('gulp-cached');
var livereload = require('gulp-livereload');
var paths = require('./helpers/paths.js');

gulp.task('copyJs', () => {
  return gulp.src(
    [
      path.join(paths.src.root, 'polyfills.js')
    ])
    .pipe(cache('copyJs'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyHtml', () => {
  return gulp.src(paths.src.html)
    .pipe(cache('copyHtml'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyFonts', () => {
  return gulp.src(
    path.join(paths.src.fonts, '**', '*.*')
  )
    .pipe(cache('copyFonts'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.fonts))
    .pipe(livereload());
});

gulp.task('copyImages', () => {
  return gulp.src(
    path.join(paths.src.images, '**', '*.*')
  )
    .pipe(cache('copyImages'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.images))
    .pipe(livereload());
});
