import { StringKey } from 'typings/Helpers/KeysMatching';

export function sortByProp<
  // eslint-disable-next-line no-use-before-define
  T extends Record<K, string>,
  K extends StringKey<T>
>(sortKey: K) {
  return (a: T, b: T) => {
    return a[sortKey].localeCompare(b[sortKey], undefined, { numeric: true });
  };
}

export default sortByProp;
