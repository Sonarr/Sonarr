import React from 'react';
import Label from 'Components/Label';

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
      <Label kind="default">
        {count} {labelPlural.toLowerCase()}
      </Label>
    );
  }

  return (
    <Label kind="default">
      {count} {label.toLowerCase()}
    </Label>
  );
}
