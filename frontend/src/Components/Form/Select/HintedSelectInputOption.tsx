import classNames from 'classnames';
import React from 'react';
import EnhancedSelectInputOption, {
  EnhancedSelectInputOptionProps,
} from './EnhancedSelectInputOption';
import styles from './HintedSelectInputOption.css';

interface HintedSelectInputOptionProps
  extends Omit<EnhancedSelectInputOptionProps, 'isSelected'> {
  value: string;
  hint?: React.ReactNode;
  isSelected?: boolean;
}

function HintedSelectInputOption(props: HintedSelectInputOptionProps) {
  const {
    id,
    value,
    hint,
    depth,
    isSelected = false,
    isMobile,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputOption
      id={id}
      depth={depth}
      isSelected={isSelected}
      isMobile={isMobile}
      {...otherProps}
    >
      <div
        className={classNames(styles.optionText, isMobile && styles.isMobile)}
      >
        <div>{value}</div>

        {hint != null && <div className={styles.hintText}>{hint}</div>}
      </div>
    </EnhancedSelectInputOption>
  );
}

export default HintedSelectInputOption;
