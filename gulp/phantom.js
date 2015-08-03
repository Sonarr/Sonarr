// Switch to phantom.
// Example:
//    gulp --phantom

var phantom = false;
process.argv.forEach(function(val, index, array) {
    if (val === '--phantom') {
        phantom = true;
    }
});

console.log('Phantom:', phantom);

module.exports = phantom;