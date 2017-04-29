var gulp = require('gulp');

var less = require('gulp-less');
var postcss = require('gulp-postcss');
var sourcemaps = require('gulp-sourcemaps');
var autoprefixer = require('autoprefixer-core');
var livereload = require('gulp-livereload');

var print = require('gulp-print');
var paths = require('./paths');
var errorHandler = require('./errorHandler');

gulp.task('less', function() {

    var src = [
        paths.src.content + 'bootstrap.less',
        paths.src.content + 'theme.less',
        paths.src.content + 'overrides.less',
        paths.src.content + 'bootstrap.toggle-switch.css',
        paths.src.content + 'fullcalendar.css',
        paths.src.content + 'Messenger/messenger.css',
        paths.src.content + 'Messenger/messenger.flat.css',
        paths.src.root + 'Series/series.less',
        paths.src.root + 'Activity/activity.less',
        paths.src.root + 'AddSeries/addSeries.less',
        paths.src.root + 'Calendar/calendar.less',
        paths.src.root + 'Cells/cells.less',
        paths.src.root + 'ManualImport/manualimport.less',
        paths.src.root + 'Settings/settings.less',
        paths.src.root + 'System/Logs/logs.less',
        paths.src.root + 'System/Update/update.less',
        paths.src.root + 'System/Info/info.less'
    ];

    return gulp.src(src)
        .pipe(print())
        .pipe(sourcemaps.init())
        .pipe(less({
            dumpLineNumbers : 'false',
            compress        : true,
            yuicompress     : true,
            ieCompat        : true,
            strictImports   : true
        }))
        .pipe(postcss([ autoprefixer({ browsers: ['last 2 versions'] }) ]))
        .on('error', errorHandler.onError)
        .pipe(sourcemaps.write(paths.dest.content))
        .pipe(gulp.dest(paths.dest.content))
        .pipe(livereload());
});
