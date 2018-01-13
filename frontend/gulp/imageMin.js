var gulp = require('gulp');
var print = require('gulp-print');
var paths = require('./helpers/paths.js');

gulp.task('imageMin', () => {
  var imagemin = require('gulp-imagemin');
  return gulp.src(paths.src.images)
    .pipe(imagemin({
      progressive: false,
      optimizationLevel: 4,
      svgoPlugins: [{ removeViewBox: false }]
    }))
    .pipe(print())
    .pipe(gulp.dest(paths.src.content + 'Images/'));
});
