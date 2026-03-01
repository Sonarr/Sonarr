export type InputChanged<T = unknown> = {
  name: string;
  value: T;
};

export type InputOnChange<T> = (change: InputChanged<T>) => void;

export interface CheckInputChanged extends InputChanged<boolean> {
  shiftKey: boolean;
}

export interface FileInputChanged extends InputChanged<string> {
  files: FileList | null | undefined;
}

export interface EnhancedSelectInputChanged<T> extends InputChanged<T> {
  value: T;
  additionalProperties?: object;
}
