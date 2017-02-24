const root = './frontend/src/';

const paths = {
  src: {
    root,
    html: root + '*.html',
    scripts: root + '**/*.js',
    content: root + 'Content/',
    fonts: root + 'Content/Fonts/',
    images: root + 'Content/Images/',
    exclude: {
      libs: `!${root}JsLibraries/**`
    }
  },
  dest: {
    root: './_output/UI/',
    content: './_output/UI/Content/',
    fonts: './_output/UI/Content/Fonts/',
    images: './_output/UI/Content/Images/'
  }
};

module.exports = paths;
