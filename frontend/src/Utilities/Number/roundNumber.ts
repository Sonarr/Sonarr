export default function roundNumber(input: number, decimalPlaces = 1) {
  const multiplier = Math.pow(10, decimalPlaces);

  return Math.round(input * multiplier) / multiplier;
}
