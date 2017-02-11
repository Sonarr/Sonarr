const gulpUtil = require('gulp-util');

module.exports = function errorHandler(error) {
  gulpUtil.log(gulpUtil.colors.red(`Error (${error.plugin}): ${error.message}`));
  this.emit('end');
};
