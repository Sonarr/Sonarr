var args = require('yargs').argv;
// Switch to phantom.
// Example:
//    gulp --phantom

var phantom = !!args.phantom;

console.log('Phantom:', phantom);

module.exports = phantom;