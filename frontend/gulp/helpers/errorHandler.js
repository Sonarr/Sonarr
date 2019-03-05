const colors = require('ansi-colors');

module.exports = function errorHandler(error) {
  console.log(colors.red(`Error (${error.plugin}): ${error.message}`));
  this.emit('end');
};
