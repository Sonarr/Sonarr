export interface SelectStateInputProps<T extends number | string = number> {
  id: T;
  value: boolean | null;
  shiftKey: boolean;
}
