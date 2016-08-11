"use strict";

var gulp = require('gulp');
var rename = require("gulp-rename");
var yaml = require('js-yaml');
var through2 = require('through2');

var paths = require('./paths.js');

gulp.task('metaOption', function() {
    through2
    return gulp.src('gulp/meta-option.yml')
        .pipe(through2.obj(function (file, enc, callback) {
            var data = parseConfig(String(file.contents));
            file.contents = new Buffer(JSON.stringify(data));
            this.push(file);
            callback();
        }))

        .pipe(rename({
            dirname: "",
            basename: "metaOption",
            extname: ".json"
        }))
        .pipe(gulp.dest(paths.dest.content));
});

var parseConfig = function parseConfig(content) {

    var rawConfig = yaml.safeLoad(content);
    var config = {};

    if (rawConfig) {
        for (var tabName in rawConfig) {

            if (! (tabName in config)) {
                config[tabName] = {};
            }

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
                    'readonly': false,
                    'visible': true
                };

                // parse options:
                for (var opt in optarray) {
                    var s = (opt || '').toString().toLowerCase();
                    switch(s) {
                        case 'ro':
                        case 'readonly':
                        case 'read-only':
                            optobj.readonly = true;
                            break;
                        case 'rw':
                        case 'read-write':
                        case 'readwrite':
                            optobj.readonly = false;
                            break;
                        case 'show':
                        case 'visible':
                            optobj.visible = true;
                            break;
                        case 'hide':
                        case 'hidden':
                            optobj.visible = false;
                            break;
                    }
                }

                config[tabName][optName] = optobj;

            }
        }
    }

    return config;
}
