const path = require('path');
const gulp = require('gulp');
const print = require('gulp-print').default;
const cache = require('gulp-cached');
const livereload = require('gulp-livereload');
const paths = require('./helpers/paths.js');

gulp.task('copyJs', () => {
  return gulp.src(
    [
      path.join(paths.src.root, 'polyfills.js')
    ], { base: paths.src.root })
    .pipe(cache('copyJs'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyHtml', () => {
  return gulp.src(paths.src.html, { base: paths.src.root })
    .pipe(cache('copyHtml'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyFonts', () => {
  return gulp.src(
    path.join(paths.src.fonts, '**', '*.*'), { base: paths.src.root }
  )
    .pipe(cache('copyFonts'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});

gulp.task('copyImages', () => {
  return gulp.src(
    path.join(paths.src.images, '**', '*.*'), { base: paths.src.root }
  )
    .pipe(cache('copyImages'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});
