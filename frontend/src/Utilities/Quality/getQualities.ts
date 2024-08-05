import Quality from 'Quality/Quality';
import { QualityProfileQualityItem } from 'typings/QualityProfile';

export default function getQualities(qualities?: QualityProfileQualityItem[]) {
  if (!qualities) {
    return [];
  }

  return qualities.reduce<Quality[]>((acc, item) => {
    if (item.quality) {
      acc.push(item.quality);
    } else {
      const groupQualities = item.items.reduce<Quality[]>((acc, i) => {
        if (i.quality) {
          acc.push(i.quality);
        }

        return acc;
      }, []);

      acc.push(...groupQualities);
    }

    return acc;
  }, []);
}
