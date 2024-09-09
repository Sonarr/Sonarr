import classNames from 'classnames';
import React, { SyntheticEvent, useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import CheckInput from '../CheckInput';
import styles from './EnhancedSelectInputOption.css';

function handleCheckPress() {
  // CheckInput requires a handler. Swallow the change event because onPress will already handle it via event propagation.
}

export interface EnhancedSelectInputOptionProps {
  className?: string;
  id: string | number;
  depth?: number;
  isSelected: boolean;
  isDisabled?: boolean;
  isHidden?: boolean;
  isMultiSelect?: boolean;
  isMobile: boolean;
  children: React.ReactNode;
  onSelect: (...args: unknown[]) => unknown;
}

function EnhancedSelectInputOption({
  className = styles.option,
  id,
  depth = 0,
  isSelected,
  isDisabled = false,
  isHidden = false,
  isMultiSelect = false,
  isMobile,
  children,
  onSelect,
}: EnhancedSelectInputOptionProps) {
  const handlePress = useCallback(
    (event: SyntheticEvent) => {
      event.preventDefault();

      onSelect(id);
    },
    [id, onSelect]
  );

  return (
    <Link
      className={classNames(
        className,
        isSelected && !isMultiSelect && styles.isSelected,
        isDisabled && !isMultiSelect && styles.isDisabled,
        isHidden && styles.isHidden,
        isMobile && styles.isMobile
      )}
      component="div"
      isDisabled={isDisabled}
      onPress={handlePress}
    >
      {depth !== 0 && <div style={{ width: `${depth * 20}px` }} />}

      {isMultiSelect && (
        <CheckInput
          className={styles.optionCheckInput}
          containerClassName={styles.optionCheck}
          name={`select-${id}`}
          value={isSelected}
          isDisabled={isDisabled}
          onChange={handleCheckPress}
        />
      )}

      {children}

      {isMobile && (
        <div className={styles.iconContainer}>
          <Icon name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE} />
        </div>
      )}
    </Link>
  );
}

export default EnhancedSelectInputOption;
