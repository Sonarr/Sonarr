import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MenuItem, { MenuItemProps } from 'Components/Menu/MenuItem';
import { selectIndexerSchema } from 'Store/Actions/settingsActions';

interface AddIndexerPresetMenuItemProps
  extends Omit<MenuItemProps, 'children'> {
  name: string;
  implementation: string;
  implementationName: string;
  onPress: () => void;
}

function AddIndexerPresetMenuItem({
  name,
  implementation,
  implementationName,
  onPress,
  ...otherProps
}: AddIndexerPresetMenuItemProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(
      selectIndexerSchema({
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

export default AddIndexerPresetMenuItem;
