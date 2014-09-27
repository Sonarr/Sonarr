var gulp = require('gulp');
var runSequence = require('run-sequence');

require('./clean');
require('./requirejs');
require('./less');
require('./handlebars');
require('./copy');

gulp.task('build', function () {
    return  runSequence('clean',
        ['requireJs', 'less', 'handlebars', 'copyIndex', 'copyContent']);
});