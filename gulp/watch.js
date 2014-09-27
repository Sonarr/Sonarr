var gulp = require('gulp');
//var livereload = require('gulp-livereload');


var paths = require('./paths.js');

require('./jshint.js');
require('./handlebars.js');
require('./less.js');
require('./copy.js');


gulp.task('watch', ['jshint', 'handlebars', 'less', 'copyJs','copyIndex', 'copyContent'], function () {
    gulp.watch([paths.src.scripts, paths.src.exclude.libs], ['jshint', 'copyJs']);
    gulp.watch(paths.src.templates, ['handlebars']);
    gulp.watch([paths.src.less, paths.src.exclude.libs], ['less']);
    gulp.watch([paths.src.index], ['copyIndex']);
    gulp.watch([paths.src.content + '**/*.*', '!**/*.less'], ['copyContent']);
});

gulp.task('liveReload', ['jshint', 'handlebars', 'less', 'copyJs'], function () {
    var server = livereload();
    gulp.watch([
        'app/**/*.js',
        'app/**/*.css',
        'app/index.html'
    ]).on('change', function (file) {
        server.changed(file.path);
    });
});