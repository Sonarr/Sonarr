export interface FieldSelectOption<T> {
  value: T;
  name: string;
  order: number;
  hint?: string;
  parentValue?: T;
  isDisabled?: boolean;
  additionalProperties?: Record<string, unknown>;
}

interface Field {
  order: number;
  name: string;
  label: string;
  value: boolean | number | string | number[];
  section: string;
  hidden: 'hidden' | 'hiddenIfNotSet' | 'visible';
  type: string;
  advanced: boolean;
  privacy: string;
}

export default Field;
