"use strict";

var fs = require('fs');
var path = require('path');

var yaml = require('js-yaml');
var htmlreplace = require('gulp-html-replace');
//var _ = require('lodash');

var configFileName = 'meta-option.yml';
var configPath = path.join(__dirname, configFileName);

var main = function hiddenOptionMeta_main() {
    var rawConfig = {};

    try {
        rawConfig = yaml.safeLoad(
            fs.readFileSync(configPath, 'utf8')
        );
    } catch (e) {

        if ('ENOENT' === e.code) {
            // no config file, nothing to do...
            console.log('Meta options file NOT FOUND: [', configFileName, ']');
        } else {
            throw e;
        }
    }

    var config = {};

    if (rawConfig) {
        for (var tabName in rawConfig) {
            for (var optName in rawConfig[tabName]) {
                if (rawConfig[tabName][optName]) {
                    // there is truthy value, so currently
                    // we could just skip this key
                } else {
                    config[tabName + '-' + optName] = '';
                }
            }
        }

        //console.log(config);
    }

    return htmlreplace(config, {
        keepUnassigned: true
    });
}

exports = module.exports = main;
