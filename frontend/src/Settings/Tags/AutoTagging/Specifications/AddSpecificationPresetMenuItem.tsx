import React, { useCallback } from 'react';
import MenuItem from 'Components/Menu/MenuItem';

interface AddSpecificationPresetMenuItemProps {
  name: string;
  implementation: string;
  onPress: ({
    name,
    implementation,
  }: {
    name: string;
    implementation: string;
  }) => void;
}

export default function AddSpecificationPresetMenuItem({
  name,
  implementation,
  onPress,
  ...otherProps
}: AddSpecificationPresetMenuItemProps) {
  const handlePress = useCallback(() => {
    onPress({
      name,
      implementation,
    });
  }, [name, implementation, onPress]);

  return (
    <MenuItem {...otherProps} onPress={handlePress}>
      {name}
    </MenuItem>
  );
}
