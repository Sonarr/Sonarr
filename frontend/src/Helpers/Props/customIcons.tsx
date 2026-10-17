import { LucideIcon, LucideProps } from 'lucide-react';
import React, { forwardRef, ReactNode } from 'react';

function customIcon(displayName: string, children: ReactNode): LucideIcon {
  const Component = forwardRef<SVGSVGElement, LucideProps>(
    ({ size = 24, className }, ref) => (
      <svg
        ref={ref}
        className={className}
        width={size}
        height={size}
        viewBox="0 0 24 24"
        fill="currentColor"
        stroke="none"
        aria-hidden={true}
      >
        {children}
      </svg>
    )
  );

  Component.displayName = displayName;

  return Component as unknown as LucideIcon;
}

export const CalendarPremiere = customIcon(
  'CalendarPremiere',
  <path d="M8 5.5a1 1 0 0 1 1.5-.87l9 6.5a1 1 0 0 1 0 1.74l-9 6.5A1 1 0 0 1 8 18.5z" />
);

export const CalendarSeasonFinale = customIcon(
  'CalendarSeasonFinale',
  <rect x="6" y="6" width="12" height="12" rx="2.5" />
);

export const CalendarSeriesFinale = customIcon(
  'CalendarSeriesFinale',
  <>
    <rect
      x="3"
      y="3"
      width="18"
      height="18"
      rx="4.5"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.2"
    />
    <rect x="8.5" y="8.5" width="7" height="7" rx="1.5" />
  </>
);

export const CalendarSpecial = customIcon(
  'CalendarSpecial',
  <path d="M12 3.2l2.5 5.1 5.6.8-4.05 3.95.96 5.58L12 16.98l-5.01 2.63.96-5.58L3.9 9.1l5.6-.8z" />
);

export const CalendarCutoffNotMet = customIcon(
  'CalendarCutoffNotMet',
  <path d="M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zm0 4.2l4.2 5.2h-2.7v3.9h-3v-3.9H7.8z" />
);
