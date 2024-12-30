import Quality from 'Quality/Quality';
import { QualityProfileItems } from 'typings/QualityProfile';

export default function getQualities(qualities?: QualityProfileItems) {
  if (!qualities) {
    return [];
  }

  return qualities.reduce<Quality[]>((acc, item) => {
    if ('quality' in item) {
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
