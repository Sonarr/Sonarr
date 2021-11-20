const reload = require('require-nocache')(module);

const cssVarsFiles = [
  './src/Styles/Variables/dimensions',
  './src/Styles/Variables/fonts',
  './src/Styles/Variables/animations',
  './src/Styles/Variables/zIndexes'
].map(require.resolve);

const mixinsFiles = [
  'frontend/src/Styles/Mixins/cover.css',
  'frontend/src/Styles/Mixins/linkOverlay.css',
  'frontend/src/Styles/Mixins/scroller.css',
  'frontend/src/Styles/Mixins/truncate.css'
];

module.exports = {
  plugins: [
    ['postcss-mixins', {
      mixinsFiles
    }],
    ['postcss-simple-vars', {
      variables: () =>
        cssVarsFiles.reduce((acc, vars) => {
          return Object.assign(acc, reload(vars));
        }, {})
    }],
    'postcss-color-function',
    'postcss-nested'
  ]
};
