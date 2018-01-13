const gulp = require('gulp');
const livereload = require('gulp-livereload');
const watch = require('gulp-watch');
const paths = require('./helpers/paths.js');

require('./copy.js');
require('./webpack.js');

function watchTask(glob, task) {
  const options = {
    name: `watch: ${task}`,
    verbose: true
  };
  return watch(glob, options, () => {
    gulp.start(task);
  });
}

gulp.task('watch', ['copyHtml', 'copyFonts', 'copyImages', 'copyJs'], () => {
  livereload.listen({ start: true });

  gulp.start('webpackWatch');

  watchTask(paths.src.html, 'copyHtml');
  watchTask(`${paths.src.fonts}**/*.*`, 'copyFonts');
  watchTask(`${paths.src.images}**/*.*`, 'copyImages');
});
