const gulp = require('gulp');
const del = require('del');

const paths = require('./helpers/paths');

gulp.task('clean', () => {
  return del([paths.dest.root]);
});
