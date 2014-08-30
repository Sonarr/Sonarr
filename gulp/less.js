var gulp = require('gulp');
var less = require('gulp-less');
var print = require('gulp-print');

var paths = require('./paths');
var errorHandler = require('./errorHandler');

gulp.task('less', function () {
    return  gulp.src([
            paths.src.content + 'bootstrap.less',
            paths.src.content + 'theme.less',
            paths.src.content + 'overrides.less',
            paths.src.root + 'Series/series.less',
            paths.src.root + 'History/history.less',
            paths.src.root + 'AddSeries/addSeries.less',
            paths.src.root + 'Calendar/calendar.less',
            paths.src.root + 'Cells/cells.less',
            paths.src.root + 'Settings/settings.less',
            paths.src.root + 'System/Logs/logs.less',
            paths.src.root + 'System/Update/update.less',
    ])
        .pipe(print())
        .pipe(less({
            dumpLineNumbers: 'false',
            compress: true,
            yuicompress: true,
            ieCompat: true,
            strictImports: true
        }))
        .on('error', errorHandler.onError)
        .pipe(gulp.dest(paths.dest.content));
});