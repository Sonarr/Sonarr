const path = require('path');
const gulp = require('gulp');
const print = require('gulp-print').default;
const cache = require('gulp-cached');
const flatten = require('gulp-flatten');
const livereload = require('gulp-livereload');
const paths = require('./helpers/paths.js');

gulp.task('copyJs', () => {
  return gulp.src(
    [
      path.join(paths.src.root, 'polyfills.js')
    ])
    .pipe(cache('copyJs'))
    .pipe(print())
    .pipe(flatten())
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
    .pipe(flatten({ subPath: 2 }))
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyImages', () => {
  return gulp.src(
    path.join(paths.src.images, '**', '*.*')
  )
    .pipe(cache('copyImages'))
    .pipe(print())
    .pipe(flatten({ subPath: 2 }))
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});
