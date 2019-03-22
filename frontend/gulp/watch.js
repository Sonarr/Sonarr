const gulp = require('gulp');
const livereload = require('gulp-livereload');
const gulpWatch = require('gulp-watch');
const paths = require('./helpers/paths.js');

require('./copy.js');
require('./webpack.js');

function watch() {
  livereload.listen({ start: true });

  gulp.task('webpackWatch')();
  gulpWatch(paths.src.html, gulp.series('copyHtml'));
  gulpWatch(`${paths.src.fonts}**/*.*`, gulp.series('copyFonts'));
  gulpWatch(`${paths.src.images}**/*.*`, gulp.series('copyImages'));
}

gulp.task('watch', gulp.series('build', watch));
