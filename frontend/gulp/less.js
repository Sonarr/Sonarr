const gulp = require('gulp');

const less = require('gulp-less');
const postcss = require('gulp-postcss');
const sourcemaps = require('gulp-sourcemaps');
const autoprefixer = require('autoprefixer');
const livereload = require('gulp-livereload');
const path = require('path');

const print = require('gulp-print');
const paths = require('./helpers/paths');
const errorHandler = require('./helpers/errorHandler');

gulp.task('less', () => {
  const src = [
    path.join(paths.src.content, 'Bootstrap', 'bootstrap.less'),
    path.join(paths.src.content, 'Vendor', 'vendor.less'),
    path.join(paths.src.content, 'sonarr.less')
  ];

  return gulp.src(src)
    .pipe(print())
    .pipe(sourcemaps.init())
    .pipe(less({
      paths: [paths.src.root],
      dumpLineNumbers: 'false',
      compress: true,
      yuicompress: true,
      ieCompat: true,
      strictImports: true
    }))
    .on('error', errorHandler)
    .pipe(postcss([autoprefixer({
      browsers: ['last 2 versions']
    })]))
    .on('error', errorHandler)

    // not providing a path will cause the source map
    // to be embeded. which makes livereload much happier
    // since it doesn't reload the whole page to load the map.
    // this should be switched to sourcemaps.write('./') for production builds
    .pipe(sourcemaps.write())
    .pipe(gulp.dest(paths.dest.content))
    .on('error', errorHandler)
    .pipe(livereload());
});
