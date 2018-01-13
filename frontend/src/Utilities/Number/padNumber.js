function padNumber(input, width, paddingCharacter = 0) {
  if (input == null) {
    return '';
  }

  input = `${input}`;
  return input.length >= width ? input : new Array(width - input.length + 1).join(paddingCharacter) + input;
}

export default padNumber;
