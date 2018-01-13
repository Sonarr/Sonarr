import PropTypes from 'prop-types';
import React from 'react';
import { icons, sortDirections } from 'Helpers/Props';
import SelectedMenuItem from './SelectedMenuItem';

function SortMenuItem(props) {
  const {
    name,
    sortKey,
    sortDirection,
    ...otherProps
  } = props;

  const isSelected = name === sortKey;

  return (
    <SelectedMenuItem
      name={name}
      selectedIconName={sortDirection === sortDirections.ASCENDING ? icons.SORT_ASCENDING : icons.SORT_DESCENDING}
      isSelected={isSelected}
      {...otherProps}
    />
  );
}

SortMenuItem.propTypes = {
  name: PropTypes.string,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  onPress: PropTypes.func.isRequired
};

SortMenuItem.defaultProps = {
  name: null
};

export default SortMenuItem;
