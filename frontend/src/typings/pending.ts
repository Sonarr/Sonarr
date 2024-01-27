export interface Pending<T> {
  value: T;
  errors: any[];
  warnings: any[];
}

export type PendingSection<T> = {
  [K in keyof T]: Pending<T[K]>;
};
