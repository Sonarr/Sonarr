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

module.exports = paths;
