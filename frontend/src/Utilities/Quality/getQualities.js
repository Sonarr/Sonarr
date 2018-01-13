export default function getQualities(qualities) {
  if (!qualities) {
    return [];
  }

  return qualities.reduce((acc, item) => {
    if (item.quality) {
      acc.push(item.quality);
    } else {
      const groupQualities = item.items.map((i) => i.quality);
      acc.push(...groupQualities);
    }

    return acc;
  }, []);
}
