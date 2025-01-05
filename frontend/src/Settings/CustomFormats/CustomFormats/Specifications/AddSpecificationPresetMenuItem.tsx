import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MenuItem from 'Components/Menu/MenuItem';
import { selectCustomFormatSpecificationSchema } from 'Store/Actions/settingsActions';

interface AddSpecificationPresetMenuItemProps {
  name: string;
  implementation: string;
  implementationName: string;
  onPress: () => void;
}

function AddSpecificationPresetMenuItem({
  name,
  implementation,
  implementationName,
  onPress,
  ...otherProps
}: AddSpecificationPresetMenuItemProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(
      selectCustomFormatSpecificationSchema({
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

export default AddSpecificationPresetMenuItem;
