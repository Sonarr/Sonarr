var phantom = require('./phantom');

var paths = {
    src  : {
        root      : './src/UI/',
        templates : './src/UI/**/*.hbs',
        html      : './src/UI/*.html',
        partials  : './src/UI/**/*Partial.hbs',
        scripts   : './src/UI/**/*.js',
        less      : ['./src/UI/**/*.less'],
        content   : './src/UI/Content/',
        images    : './src/UI/Content/Images/**/*',
        exclude   : {
            libs : '!./src/UI/JsLibraries/**'
        }
    },
    dest : {
        root    : './_output/UI/',
        content : './_output/UI/Content/'
    }
};

if (phantom) {
    paths = {
        src  : {
            root      : './UI.Phantom/',
            templates : './UI.Phantom/**/*.hbs',
            html      : './UI.Phantom/*.html',
            partials  : './UI.Phantom/**/*Partial.hbs',
            scripts   : './UI.Phantom/**/*.js',
            less      : ['./UI.Phantom/**/*.less'],
            content   : './UI.Phantom/Content/',
            images    : './UI.Phantom/Content/Images/**/*',
            exclude   : {
                libs : '!./UI.Phantom/JsLibraries/**'
            }
        },
        dest : {
            root    : './_output/UI.Phantom/',
            content : './_output/UI.Phantom/Content/'
        }
    };
}

module.exports = paths;
