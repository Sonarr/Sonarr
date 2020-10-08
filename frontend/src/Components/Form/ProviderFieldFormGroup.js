import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function getType({ type, selectOptionsProviderAction }) {
  switch (type) {
    case 'captcha':
      return inputTypes.CAPTCHA;
    case 'checkbox':
      return inputTypes.CHECK;
    case 'device':
      return inputTypes.DEVICE;
    case 'password':
      return inputTypes.PASSWORD;
    case 'number':
      return inputTypes.NUMBER;
    case 'path':
      return inputTypes.PATH;
    case 'filePath':
      return inputTypes.PATH;
    case 'select':
      if (selectOptionsProviderAction) {
        return inputTypes.DYNAMIC_SELECT;
      }
      return inputTypes.SELECT;
    case 'tag':
      return inputTypes.TEXT_TAG;
    case 'textbox':
      return inputTypes.TEXT;
    case 'oAuth':
      return inputTypes.OAUTH;
    default:
      return inputTypes.TEXT;
  }
}

function getSelectValues(selectOptions) {
  if (!selectOptions) {
    return;
  }

  return _.reduce(selectOptions, (result, option) => {
    result.push({
      key: option.value,
      value: option.name,
      hint: option.hint
    });

    return result;
  }, []);
}

function ProviderFieldFormGroup(props) {
  const {
    advancedSettings,
    name,
    label,
    helpText,
    helpLink,
    value,
    type,
    advanced,
    hidden,
    pending,
    errors,
    warnings,
    selectOptions,
    onChange,
    ...otherProps
  } = props;

  if (
    hidden === 'hidden' ||
    (hidden === 'hiddenIfNotSet' && !value)
  ) {
    return null;
  }

  return (
    <FormGroup
      advancedSettings={advancedSettings}
      isAdvanced={advanced}
    >
      <FormLabel>{label}</FormLabel>

      <FormInputGroup
        type={getType(props)}
        name={name}
        label={label}
        helpText={helpText}
        helpLink={helpLink}
        value={value}
        values={getSelectValues(selectOptions)}
        errors={errors}
        warnings={warnings}
        pending={pending}
        includeFiles={type === 'filePath' ? true : undefined}
        onChange={onChange}
        {...otherProps}
      />
    </FormGroup>
  );
}

const selectOptionsShape = {
  name: PropTypes.string.isRequired,
  value: PropTypes.number.isRequired,
  hint: PropTypes.string
};

ProviderFieldFormGroup.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  helpText: PropTypes.string,
  helpLink: PropTypes.string,
  value: PropTypes.any,
  type: PropTypes.string.isRequired,
  advanced: PropTypes.bool.isRequired,
  hidden: PropTypes.string,
  pending: PropTypes.bool.isRequired,
  errors: PropTypes.arrayOf(PropTypes.object).isRequired,
  warnings: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectOptions: PropTypes.arrayOf(PropTypes.shape(selectOptionsShape)),
  selectOptionsProviderAction: PropTypes.string,
  onChange: PropTypes.func.isRequired
};

ProviderFieldFormGroup.defaultProps = {
  advancedSettings: false
};

export default ProviderFieldFormGroup;
