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
            root      : './src/UI.Phantom/',
            templates : './src/UI.Phantom/**/*.hbs',
            html      : './src/UI.Phantom/*.html',
            partials  : './src/UI.Phantom/**/*Partial.hbs',
            scripts   : './src/UI.Phantom/**/*.js',
            less      : ['./src/UI.Phantom/**/*.less'],
            content   : './src/UI.Phantom/Content/',
            images    : './src/UI.Phantom/Content/Images/**/*',
            exclude   : {
                libs : '!./src/UI.Phantom/JsLibraries/**'
            }
        },
        dest : {
            root    : './_output/UI.Phantom/',
            content : './_output/UI.Phantom/Content/'
        }
    };
}

module.exports = paths;
