import React, { useCallback } from 'react';
import MenuItem from 'Components/Menu/MenuItem';

interface AddSpecificationPresetMenuItemProps {
  name: string;
  implementation: string;
  onPress: (selected: { implementation: string; presetName: string }) => void;
}

function AddSpecificationPresetMenuItem({
  name,
  implementation,
  onPress,
}: AddSpecificationPresetMenuItemProps) {
  const handlePress = useCallback(() => {
    onPress({ implementation, presetName: name });
  }, [name, implementation, onPress]);

  return <MenuItem onPress={handlePress}>{name}</MenuItem>;
}

export default AddSpecificationPresetMenuItem;
