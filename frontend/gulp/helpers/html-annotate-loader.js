const path = require('path');
const rootPath = path.resolve(__dirname + '/../../src/');
module.exports = function(source) {
  if (this.cacheable) {
    this.cacheable();
  }

  const resourcePath = this.resourcePath.replace(rootPath, '');
  const wrappedSource =`
  <!-- begin ${resourcePath} -->
    ${source}
  <!-- end ${resourcePath} -->`;

  return wrappedSource;
};
