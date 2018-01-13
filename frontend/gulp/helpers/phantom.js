var phantom = false;
process.argv.forEach((val) => {
  if (val === '--phantom') {
    phantom = true;
  }
});

console.log('Phantom:', phantom);

module.exports = phantom;
