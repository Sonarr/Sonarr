"use strict";

var gulp = require('gulp');
var rename = require("gulp-rename");
var yaml = require('js-yaml');
var through2 = require('through2');

var paths = require('./paths.js');

gulp.task('metaOption', function() {
    return gulp.src('meta.option.yml')
        .pipe(through2.obj(function (file, enc, callback) {
            var data = parseConfigYml(String(file.contents));
            file.contents = new Buffer(JSON.stringify(data));
            this.push(file);
            callback();
        }))

        .pipe(rename({
            dirname: '',
            basename: 'metaOption',
            extname: '.json'
        }))
        .pipe(gulp.dest(paths.dest.content));
});

var parseConfigYml = function parseConfigYml(content) {

    var rawConfig = yaml.safeLoad(content);
    var config = {};

    if (rawConfig) {
        for (var tabName in rawConfig) {

            for (var optName in rawConfig[tabName]) {

                var o = rawConfig[tabName][optName];
                var optarray = [];
                if ("string" === typeof(o)) {
                    optarray.push(o);
                } else if( Object.prototype.toString.call( o ) === '[object Array]') {
                    optarray = o;
                } else {
                    console.error('Unknown format of the option: [' + tabName + '][' + optName + ']');
                }

                var optobj = {
                    name: optName,
                    writable: true,
                    readable: true
                };

                // parse options:
                for (var opt in optarray) {
                    var s = (optarray[opt] || '').toString().toLowerCase();
                    switch(s) {
                        // --------------------------
                        // PURE:
                        case 'readable':
                        case 'r+':
                            optobj.readable = true;
                            break;
                        case 'not-readable':
                        case 'r-':
                            optobj.readable = false;
                            break;
                        case 'writable':
                        case 'w+':
                            optobj.writable = true;
                            break;
                        case 'not-writable':
                        case 'w-':
                            optobj.writable = false;
                            break;

                        // --------------------------
                        // PRESETS:
                        case 'ro':
                        case 'readonly':
                        case 'read-only':
                            optobj.readable = true;
                            optobj.writable = false;
                            break;
                        case 'rw':
                        case 'show':
                        case 'read-write':
                        case 'readwrite':
                            optobj.readable = true;
                            optobj.writable = true;
                            break;
                        case 'hide':
                        case 'hidden':
                            optobj.readable = false;
                            optobj.writable = false;
                            break;
                        default:
                            console.warn('Unrecognized value for the option: [' + tabName + '][' + optName + '] : ' + s);
                            break;
                    }
                }

                config[optName] = optobj;

            }
        }
    }

    return config;
}
