type KeysMatching<T, V> = {
  [K in keyof T]-?: T[K] extends V ? K : never;
}[keyof T];

export type StringKey<T> = KeysMatching<T, string>;

export default KeysMatching;
