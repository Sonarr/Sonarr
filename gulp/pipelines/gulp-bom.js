var replace = require('gulp-replace');
module.exports = function() {
   return replace(/^\uFEFF/, '');
};