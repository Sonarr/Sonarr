import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MenuItem from 'Components/Menu/MenuItem';
import { selectNotificationSchema } from 'Store/Actions/settingsActions';

interface AddNotificationPresetMenuItemProps {
  name: string;
  implementation: string;
  implementationName: string;
  onPress: () => void;
}

function AddNotificationPresetMenuItem({
  name,
  implementation,
  implementationName,
  onPress,
  ...otherProps
}: AddNotificationPresetMenuItemProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(
      selectNotificationSchema({
        implementation,
        implementationName,
        presetName: name,
      })
    );

    onPress();
  }, [name, implementation, implementationName, dispatch, onPress]);

  return (
    <MenuItem {...otherProps} onPress={handlePress}>
      {name}
    </MenuItem>
  );
}

export default AddNotificationPresetMenuItem;
