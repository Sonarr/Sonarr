import classNames from 'classnames';
import React from 'react';
import Link from 'Components/Link/Link';
import styles from './OverrideMatchData.css';

interface OverrideMatchDataProps {
  value?: string | number | JSX.Element | JSX.Element[];
  isDisabled?: boolean;
  isOptional?: boolean;
  onPress: () => void;
}

function OverrideMatchData(props: OverrideMatchDataProps) {
  const { value, isDisabled = false, isOptional, onPress } = props;

  return (
    <Link className={styles.link} isDisabled={isDisabled} onPress={onPress}>
      {(value == null || (Array.isArray(value) && value.length === 0)) &&
      !isDisabled ? (
        <span
          className={classNames(
            styles.placeholder,
            isOptional && styles.optional
          )}
        >
          &nbsp;
        </span>
      ) : (
        value
      )}
    </Link>
  );
}

export default OverrideMatchData;
