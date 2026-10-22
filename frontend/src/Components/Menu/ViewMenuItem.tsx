import React from 'react';
import SelectedMenuItem, { SelectedMenuItemProps } from './SelectedMenuItem';

interface ViewMenuItemProps extends Omit<SelectedMenuItemProps, 'isSelected'> {
  name?: string;
  selectedView: string;
  children: React.ReactNode;
  onPress: (view: string) => void;
}

function ViewMenuItem({
  name,
  selectedView,
  ...otherProps
}: ViewMenuItemProps) {
  const isSelected = name === selectedView;

  return (
    <SelectedMenuItem {...otherProps} name={name} isSelected={isSelected} />
  );
}

export default ViewMenuItem;
