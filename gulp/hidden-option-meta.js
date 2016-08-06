"use strict";

var yaml = require('js-yaml');
var fs = require('fs');

var main = function hiddenOptionMeta_main() {
    var config = {};

    try {
        config = yaml.safeLoad(
            fs.readFileSync('./hidden-option-meta.yml', 'utf8')
        );
    } catch (e) {
        if ('ENOENT' === e.code) {
            // no config file, nothing to do...
        } else {
            throw e;
        }
    }

/*
    config = {
            'general-bindAddress': '',
            'general-sslPort': '',
    };
*/

console.log(config);

    return config;
}

exports = module.exports = main;
