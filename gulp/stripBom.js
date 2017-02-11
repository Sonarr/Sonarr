var gulp = require('gulp');
var paths = require('./paths.js');
var stripbom = require('gulp-stripbom');

var stripBom = function(dest) {
  gulp.src([paths.src.scripts, paths.src.exclude.libs])
    .pipe(stripbom({
      showLog: false
    }))
    .pipe(gulp.dest(dest));

  gulp.src(paths.src.less)
    .pipe(stripbom({
      showLog: false
    }))
    .pipe(gulp.dest(dest));

  gulp.src(paths.src.templates)
    .pipe(stripbom({
      showLog: false
    }))
    .pipe(gulp.dest(dest));
};

gulp.task('stripBom', function() {
  stripBom(paths.src.root);
});
