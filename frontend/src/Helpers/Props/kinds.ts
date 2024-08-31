export const DANGER = 'danger';
export const DEFAULT = 'default';
export const DISABLED = 'disabled';
export const INFO = 'info';
export const INVERSE = 'inverse';
export const PINK = 'pink';
export const PRIMARY = 'primary';
export const PURPLE = 'purple';
export const SUCCESS = 'success';
export const WARNING = 'warning';

export const all = [
  DANGER,
  DEFAULT,
  DISABLED,
  INFO,
  INVERSE,
  PINK,
  PRIMARY,
  PURPLE,
  SUCCESS,
  WARNING,
] as const;

export type Kind =
  | 'danger'
  | 'default'
  | 'disabled'
  | 'info'
  | 'inverse'
  | 'pink'
  | 'primary'
  | 'purple'
  | 'success'
  | 'warning';
