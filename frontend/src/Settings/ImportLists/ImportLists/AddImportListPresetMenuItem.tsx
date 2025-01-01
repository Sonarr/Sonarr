import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MenuItem, { MenuItemProps } from 'Components/Menu/MenuItem';
import { selectImportListSchema } from 'Store/Actions/settingsActions';

interface AddImportListPresetMenuItemProps
  extends Omit<MenuItemProps, 'children'> {
  name: string;
  implementation: string;
  implementationName: string;
  minRefreshInterval: string;
  onPress: () => void;
}

function AddImportListPresetMenuItem({
  name,
  implementation,
  implementationName,
  minRefreshInterval,
  onPress,
  ...otherProps
}: AddImportListPresetMenuItemProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(
      selectImportListSchema({
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

export default AddImportListPresetMenuItem;
