/* eslint-disable no-bitwise */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import EnhancedSelectInput from './EnhancedSelectInput';
import styles from './UMaskInput.css';

const umaskOptions = [
  {
    key: '755',
    value: '755 - Owner write, Everyone else read',
    hint: 'drwxr-xr-x'
  },
  {
    key: '775',
    value: '775 - Owner & Group write, Other read',
    hint: 'drwxrwxr-x'
  },
  {
    key: '770',
    value: '770 - Owner & Group write',
    hint: 'drwxrwx---'
  },
  {
    key: '750',
    value: '750 - Owner write, Group read',
    hint: 'drwxr-x---'
  },
  {
    key: '777',
    value: '777 - Everyone write',
    hint: 'drwxrwxrwx'
  }
];

function formatPermissions(permissions) {

  const hasSticky = permissions & 0o1000;
  const hasSetGID = permissions & 0o2000;
  const hasSetUID = permissions & 0o4000;

  let result = '';

  for (let i = 0; i < 9; i++) {
    const bit = (permissions & (1 << i)) !== 0;
    let digit = bit ? 'xwr'[i % 3] : '-';
    if (i === 6 && hasSetUID) {
      digit = bit ? 's' : 'S';
    } else if (i === 3 && hasSetGID) {
      digit = bit ? 's' : 'S';
    } else if (i === 0 && hasSticky) {
      digit = bit ? 't' : 'T';
    }
    result = digit + result;
  }

  return result;
}

class UMaskInput extends Component {

  //
  // Render

  render() {
    const {
      name,
      value,
      onChange
    } = this.props;

    const valueNum = parseInt(value, 8);
    const umaskNum = 0o777 & ~valueNum;
    const umask = umaskNum.toString(8).padStart(4, '0');
    const folderNum = 0o777 & ~umaskNum;
    const folder = folderNum.toString(8).padStart(3, '0');
    const fileNum = 0o666 & ~umaskNum;
    const file = fileNum.toString(8).padStart(3, '0');

    const unit = formatPermissions(folderNum);

    const values = umaskOptions.map((v) => {
      return { ...v, hint: <span className={styles.unit}>{v.hint}</span> };
    });

    return (
      <div>
        <div className={styles.inputWrapper}>
          <div className={styles.inputUnitWrapper}>
            <EnhancedSelectInput
              name={name}
              value={value}
              values={values}
              isEditable={true}
              onChange={onChange}
            />

            <div className={styles.inputUnit}>
              d{unit}
            </div>
          </div>
        </div>
        <div className={styles.details}>
          <div>
            <label>UMask</label>
            <div className={styles.value}>{umask}</div>
          </div>
          <div>
            <label>Folder</label>
            <div className={styles.value}>{folder}</div>
            <div className={styles.unit}>d{formatPermissions(folderNum)}</div>
          </div>
          <div>
            <label>File</label>
            <div className={styles.value}>{file}</div>
            <div className={styles.unit}>{formatPermissions(fileNum)}</div>
          </div>
        </div>
      </div>
    );
  }
}

UMaskInput.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  onChange: PropTypes.func.isRequired,
  onFocus: PropTypes.func,
  onBlur: PropTypes.func
};

export default UMaskInput;
