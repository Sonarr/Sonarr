const gulp = require('gulp');
const runSequence = require('run-sequence');

require('./clean');
require('./copy');

gulp.task('build', () => {
  return runSequence('clean', [
    'webpack',
    'copyHtml',
    'copyFonts',
    'copyImages',
    'copyJs'
  ]);
});
