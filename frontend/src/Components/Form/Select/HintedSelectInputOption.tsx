import classNames from 'classnames';
import React from 'react';
import EnhancedSelectInputOption, {
  EnhancedSelectInputOptionProps,
} from './EnhancedSelectInputOption';
import styles from './HintedSelectInputOption.css';

interface HintedSelectInputOptionProps extends EnhancedSelectInputOptionProps {
  value: string;
  hint?: React.ReactNode;
}

function HintedSelectInputOption(props: HintedSelectInputOptionProps) {
  const {
    id,
    value,
    hint,
    depth,
    isSelected = false,
    isDisabled,
    isMobile,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputOption
      id={id}
      depth={depth}
      isSelected={isSelected}
      isDisabled={isDisabled}
      isHidden={isDisabled}
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

HintedSelectInputOption.defaultProps = {
  isDisabled: false,
  isHidden: false,
  isMultiSelect: false,
};

export default HintedSelectInputOption;
