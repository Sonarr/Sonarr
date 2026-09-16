const path = require('path');

const mixinsFiles = [
  'frontend/src/Styles/Mixins/cover.css',
  'frontend/src/Styles/Mixins/linkOverlay.css',
  'frontend/src/Styles/Mixins/scroller.css',
  'frontend/src/Styles/Mixins/truncate.css'
];

module.exports = {
  plugins: {
    autoprefixer: {},
    'postcss-mixins': {
      mixinsFiles
    },
    '@csstools/postcss-global-data': {
      files: [path.join(__dirname, 'src/Styles/Variables/breakpoints.css')]
    },
    'postcss-custom-media': {},
    'postcss-nested': {}
  }
};
