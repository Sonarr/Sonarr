var gulp = require('gulp');
var livereload = require('gulp-livereload');

var paths = require('./paths.js');

require('./jshint.js');
require('./handlebars.js');
require('./less.js');
require('./copy.js');
require('./webpack.js');

gulp.task('watch', ['jshint', 'handlebars', 'less', 'copyHtml', 'copyContent', 'copyJs'], function () {
  livereload.listen();
  gulp.start('webpackWatch');
  gulp.watch([paths.src.scripts, paths.src.exclude.libs], ['jshint', 'copyJs']);
  gulp.watch(paths.src.templates, ['handlebars']);
  gulp.watch([paths.src.less, paths.src.exclude.libs], ['less']);
  gulp.watch([paths.src.html], ['copyHtml']);
  gulp.watch([paths.src.content + '**/*.*', '!**/*.less'], ['copyContent']);
});