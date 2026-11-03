import classNames from 'classnames';
import React, {
  ButtonHTMLAttributes,
  MouseEvent,
  RefCallback,
  useCallback,
} from 'react';
import Icon from 'Components/Icon';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SortableHandle.module.css';

export interface SortableHandleOption {
  isDisabled?: boolean;
  label: string;
  onPress: () => void;
}

interface SortableHandleProps {
  className?: string;
  handleRef: RefCallback<Element>;
  label: string;
  options: SortableHandleOption[];
}

interface SortableHandleButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement> {
  onPress?: () => void;
}

function SortableHandleButton({
  onPress,
  ...props
}: SortableHandleButtonProps) {
  const handleClick = useCallback(
    (event: MouseEvent<HTMLButtonElement>) => {
      event.stopPropagation();
      onPress?.();
    },
    [onPress]
  );

  return <button onClick={handleClick} {...props} />;
}

function SortableHandle({
  className,
  handleRef,
  label,
  options,
}: SortableHandleProps) {
  return (
    <div className={classNames(styles.controls, className)}>
      <span
        ref={handleRef as RefCallback<HTMLSpanElement>}
        className={styles.button}
        role="presentation"
        tabIndex={-1}
        aria-hidden={true}
        title={translate('ReorderItem', { item: label })}
      >
        <Icon name={icons.GRIP} size={16} aria-hidden={true} />
      </span>

      <Menu className={styles.menu} alignMenu="right">
        <SortableHandleButton
          className={styles.button}
          type="button"
          aria-label={translate('MoveItem', { item: label })}
        >
          <Icon name={icons.REORDER} size={16} aria-hidden={true} />
        </SortableHandleButton>

        <MenuContent>
          {options.map((option) => (
            <MenuItem
              key={option.label}
              isDisabled={option.isDisabled}
              onPress={option.onPress}
            >
              {option.label}
            </MenuItem>
          ))}
        </MenuContent>
      </Menu>
    </div>
  );
}

export default SortableHandle;
