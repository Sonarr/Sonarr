import PropTypes from 'prop-types';

const settingShape = {
  value: PropTypes.oneOf([PropTypes.bool, PropTypes.number, PropTypes.string]),
  warnings: PropTypes.arrayOf(PropTypes.string).isRequired,
  errors: PropTypes.arrayOf(PropTypes.string).isRequired
};

export const arraySettingShape = {
  ...settingShape,
  value: PropTypes.array.isRequired
};

export const boolSettingShape = {
  ...settingShape,
  value: PropTypes.bool.isRequired
};

export const numberSettingShape = {
  ...settingShape,
  value: PropTypes.number.isRequired
};

export const stringSettingShape = {
  ...settingShape,
  value: PropTypes.string
};

export const tagSettingShape = {
  ...settingShape,
  value: PropTypes.arrayOf(PropTypes.number).isRequired
};

export default settingShape;
