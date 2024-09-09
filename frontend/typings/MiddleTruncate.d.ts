declare module 'react-middle-truncate' {
  import { ComponentPropsWithoutRef } from 'react';

  interface MiddleTruncateProps extends ComponentPropsWithoutRef<'div'> {
    text: string;
    ellipsis?: string;
    start?: number | RegExp | string;
    end?: number | RegExp | string;
    smartCopy?: 'all' | 'partial';
    onResizeDebounceMs?: number;
  }

  export default function MiddleTruncate(
    props: MiddleTruncateProps
  ): JSX.Element;
}
