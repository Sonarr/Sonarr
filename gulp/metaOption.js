"use strict";

var gulp = require('gulp');
var rename = require("gulp-rename");
var yaml = require('js-yaml');
var through2 = require('through2');

var paths = require('./paths.js');
var MetaOptionConfig = require('../src/UI/MetaOptionConfig');

gulp.task('metaOption', function() {
    return gulp.src('gulp/metaOption.yml')
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

                var optobj = new MetaOptionConfig();

                // parse options:
                for (var opt in optarray) {
                    var s = (optarray[opt] || '').toString().toLowerCase();
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
