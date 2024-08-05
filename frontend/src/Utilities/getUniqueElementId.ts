let i = 0;

// returns a HTML 4.0 compliant element IDs (http://stackoverflow.com/a/79022)

export default function getUniqueElementId() {
  return `id-${i++}`;
}
