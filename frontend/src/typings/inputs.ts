export type InputChanged<T = unknown> = {
  name: string;
  value: T;
};

export type CheckInputChanged = InputChanged<boolean>;
