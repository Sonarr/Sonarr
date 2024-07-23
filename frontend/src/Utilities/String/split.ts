function split(input: string, separator = ',') {
  if (!input) {
    return [];
  }

  return input.split(separator).reduce<string[]>((acc, s) => {
    if (s) {
      acc.push(s);
    }

    return acc;
  }, []);
}

export default split;
