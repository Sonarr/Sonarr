var gulp = require('gulp');
var livereload = require('gulp-livereload');
var watch = require('gulp-watch');
var paths = require('./helpers/paths.js');

require('./copy.js');
require('./webpack.js');

function watchTask(glob, task) {
  var options = {
    name: `watch: ${task}`,
    verbose: true
  };
  return watch(glob, options, () => {
    gulp.start(task);
  });
}

gulp.task('watch', ['copyHtml', 'copyFonts', 'copyImages', 'copyJs'], () => {
  livereload.listen();

  gulp.start('webpackWatch');

  watchTask(paths.src.html, 'copyHtml');
  watchTask(paths.src.fonts + '**/*.*', 'copyFonts');
  watchTask(paths.src.images + '**/*.*', 'copyImages');
});
