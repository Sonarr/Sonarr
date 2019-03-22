const loaderUtils = require('loader-utils');

module.exports = function cssVariablesLoader(source) {
  const options = loaderUtils.getOptions(this);

  options.cssVarsFiles.forEach((cssVarsFile) => {
    this.addDependency(cssVarsFile);
  });

  return source;
};
