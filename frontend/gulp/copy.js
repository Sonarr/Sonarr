const path = require('path');
const gulp = require('gulp');
const print = require('gulp-print').default;
const cache = require('gulp-cached');
const livereload = require('gulp-livereload');
const paths = require('./helpers/paths.js');

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

gulp.task('copyRobots', () => {
  return gulp.src(paths.src.robots, { base: paths.src.root })
    .pipe(cache('copyRobots'))
    .pipe(print())
    .pipe(gulp.dest(paths.dest.root))
    .pipe(livereload());
});
