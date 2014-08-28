var gulp = require('gulp');
var requirejs = require('requirejs');
var paths = require('./paths');

require('./handlebars.js');
require('./jshint.js');


gulp.task('requireJs', ['jshint'], function (cb) {

    var config = {
        mainConfigFile: 'src/UI/app.js',
        fileExclusionRegExp: /^.*\.(?!js$)[^.]+$/,
        preserveLicenseComments: false,
        dir: paths.dest.root,
        optimize: 'none',
        removeCombined: true,
        inlineText: false,
        keepBuildDir: true,
        modules: [
            {
                name: 'app',
                exclude: ['templates.js']
            }
        ]};

    requirejs.optimize(config, function (buildResponse) {
        console.log(buildResponse);
        cb();
    });

});
