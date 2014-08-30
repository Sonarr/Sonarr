var gulp = require('gulp');
var paths = require('./paths.js');
var bom = require('./pipelines/gulp-bom.js');
var gulpPrint = require('gulp-print');


var stripBom = function (dest) {
    gulp.src([paths.src.root, paths.src.exclude.libs])
        .pipe(bom())
        .pipe(gulpPrint(function (filepath) {
            return "booming: " + filepath;
        }))
        .pipe(gulp.dest(dest));

    gulp.src(paths.src.templates)
        .pipe(bom())
        .pipe(gulpPrint(function (filepath) {
            return "booming: " + filepath;
        }))
        .pipe(gulp.dest(dest));
};

gulp.task('stripBom', function () {
    stripBom(paths.src.root);
});
