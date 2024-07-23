export default function isString(possibleString: unknown) {
  return typeof possibleString === 'string' || possibleString instanceof String;
}
