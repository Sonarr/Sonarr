import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import Icon from 'Components/Icon';
import FormInputButton from './FormInputButton';
import SelectInput from './SelectInput';
import styles from './FileSelectInput.css';

function FileSelectInput(props) {
  const {
    className,
    value,
    items,
    hasError,
    hasWarning,
    isFetching,
    onRefreshPress,
    ...otherProps
  } = props;

  return (
    <div className={className}>
      <SelectInput
        className={styles.inputContainer}
        {...otherProps}
        value={value}
        values={[
          {
            key: '',
            value: 'Select Path',
            hidden: true
          },
          ...items
        ]}
        hasError={hasError}
        hasWarning={hasWarning}
      />

      <FormInputButton
        onPress={onRefreshPress}
      >
        <Icon
          name={icons.REFRESH}
          isSpinning={isFetching}
        />
      </FormInputButton>
    </div>
  );
}

FileSelectInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired
};

FileSelectInput.defaultProps = {
  className: styles.fileSelectInputWrapper
};

export default FileSelectInput;
