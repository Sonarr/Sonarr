/* eslint-disable no-bitwise */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';
import styles from './UMaskInput.css';

const umaskOptions = [
  {
    key: '755',
    get value() {
      return translate('Umask755Description', { octal: '755' });
    },
    hint: 'drwxr-xr-x'
  },
  {
    key: '775',
    get value() {
      return translate('Umask775Description', { octal: '775' });
    },
    hint: 'drwxrwxr-x'
  },
  {
    key: '770',
    get value() {
      return translate('Umask770Description', { octal: '770' });
    },
    hint: 'drwxrwx---'
  },
  {
    key: '750',
    get value() {
      return translate('Umask750Description', { octal: '750' });
    },
    hint: 'drwxr-x---'
  },
  {
    key: '777',
    get value() {
      return translate('Umask777Description', { octal: '777' });
    },
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
            <label>{translate('Umask')}</label>
            <div className={styles.value}>{umask}</div>
          </div>
          <div>
            <label>{translate('Folder')}</label>
            <div className={styles.value}>{folder}</div>
            <div className={styles.unit}>d{formatPermissions(folderNum)}</div>
          </div>
          <div>
            <label>{translate('File')}</label>
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
