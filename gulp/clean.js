var gulp = require('gulp');
var del = require('del');

var paths = require('./paths');

gulp.task('clean', function(cb) {
    del([paths.dest.root], cb);
});
