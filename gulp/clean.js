var gulp = require('gulp');
var clean = require('gulp-clean');

var paths = require('./paths');

gulp.task('clean', function () {
    return gulp.src(paths.dest.root, {read: false})
        .pipe(clean());
});
