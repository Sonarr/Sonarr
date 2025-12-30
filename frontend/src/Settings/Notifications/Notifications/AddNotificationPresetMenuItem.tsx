import React, { useCallback } from 'react';
import MenuItem from 'Components/Menu/MenuItem';
import { SelectedSchema } from 'Settings/useProviderSchema';

interface AddNotificationPresetMenuItemProps {
  name: string;
  implementation: string;
  implementationName: string;
  onPress: (selectedScehema: SelectedSchema) => void;
}

function AddNotificationPresetMenuItem({
  name,
  implementation,
  implementationName,
  onPress,
  ...otherProps
}: AddNotificationPresetMenuItemProps) {
  const handlePress = useCallback(() => {
    onPress({ implementation, implementationName, presetName: name });
  }, [name, implementation, implementationName, onPress]);

  return (
    <MenuItem {...otherProps} onPress={handlePress}>
      {name}
    </MenuItem>
  );
}

export default AddNotificationPresetMenuItem;
