const regex = /\d+/g;

function naturalExpansion(input) {
  if (!input) {
    return '';
  }

  return input.replace(regex, (n) => n.padStart(8, '0'));
}

export default naturalExpansion;
