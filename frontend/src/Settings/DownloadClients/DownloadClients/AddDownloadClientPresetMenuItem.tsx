import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MenuItem from 'Components/Menu/MenuItem';
import { selectDownloadClientSchema } from 'Store/Actions/settingsActions';

interface AddDownloadClientPresetMenuItemProps {
  name: string;
  implementation: string;
  implementationName: string;
  onPress: () => void;
}

function AddDownloadClientPresetMenuItem({
  name,
  implementation,
  implementationName,
  onPress,
  ...otherProps
}: AddDownloadClientPresetMenuItemProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(
      selectDownloadClientSchema({
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

export default AddDownloadClientPresetMenuItem;
