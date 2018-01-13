export default function isString(possibleString) {
  return typeof possibleString === 'string' || possibleString instanceof String;
}
