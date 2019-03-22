const gulp = require('gulp');

require('./clean');
require('./copy');
require('./webpack');

gulp.task('build',
  gulp.series('clean',
    gulp.parallel(
      'webpack',
      'copyHtml',
      'copyFonts',
      'copyImages',
      'copyJs'
    )
  )
);

