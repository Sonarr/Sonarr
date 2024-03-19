export interface ValidationFailure {
  propertyName: string;
  errorMessage: string;
  severity: 'error' | 'warning';
}

export interface ValidationError extends ValidationFailure {
  isWarning: false;
}

export interface ValidationWarning extends ValidationFailure {
  isWarning: true;
}

export interface Pending<T> {
  value: T;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export type PendingSection<T> = {
  [K in keyof T]: Pending<T[K]>;
};
