var gulp = require('gulp');
var runSequence = require('run-sequence');

require('./clean');
require('./less');
require('./handlebars');
require('./copy');

gulp.task('build', function() {
    return runSequence('clean', [
            'webpack',
            'less',
            'handlebars',
            'copyHtml',
            'copyContent',
            'copyJs'
        ]);
});
