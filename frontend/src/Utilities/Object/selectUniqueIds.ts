import KeysMatching from 'typings/Helpers/KeysMatching';

function selectUniqueIds<T, K>(
  items: T[],
  idProp: KeysMatching<T, K | K[]>
): K[] {
  const result = items.reduce((acc: Set<K>, item) => {
    if (!item[idProp]) {
      return acc;
    }

    const value = item[idProp] as K;

    if (Array.isArray(value)) {
      value.forEach((v) => {
        acc.add(v);
      });
    } else {
      acc.add(value);
    }

    return acc;
  }, new Set<K>());

  return Array.from(result);
}

export default selectUniqueIds;
