import React from 'react';

interface TagInUseProps {
  label: string;
  labelPlural?: string;
  count: number;
}

export default function TagInUse({ label, labelPlural, count }: TagInUseProps) {
  if (count === 0) {
    return null;
  }

  if (count > 1 && labelPlural) {
    return (
      <div>
        {count} {labelPlural.toLowerCase()}
      </div>
    );
  }

  return (
    <div>
      {count} {label.toLowerCase()}
    </div>
  );
}
