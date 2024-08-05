function convertToBytes(input: number, power: number, binaryPrefix: boolean) {
  const size = Number(input);

  if (isNaN(size)) {
    return '';
  }

  const prefix = binaryPrefix ? 1024 : 1000;
  const multiplier = Math.pow(prefix, power);
  const result = size * multiplier;

  return Math.round(result);
}

export default convertToBytes;
