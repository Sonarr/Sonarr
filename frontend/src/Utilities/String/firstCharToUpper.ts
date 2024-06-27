function firstCharToUpper(input: string) {
  if (!input) {
    return '';
  }

  return input.charAt(0).toUpperCase() + input.slice(1);
}

export default firstCharToUpper;
