import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AutoCompleteInput from './AutoCompleteInput';
import CaptchaInputConnector from './CaptchaInputConnector';
import CheckInput from './CheckInput';
import DeviceInputConnector from './DeviceInputConnector';
import DownloadClientSelectInputConnector from './DownloadClientSelectInputConnector';
import EnhancedSelectInput from './EnhancedSelectInput';
import EnhancedSelectInputConnector from './EnhancedSelectInputConnector';
import FormInputHelpText from './FormInputHelpText';
import IndexerFlagsSelectInput from './IndexerFlagsSelectInput';
import IndexerSelectInputConnector from './IndexerSelectInputConnector';
import KeyValueListInput from './KeyValueListInput';
import MonitorEpisodesSelectInput from './MonitorEpisodesSelectInput';
import MonitorNewItemsSelectInput from './MonitorNewItemsSelectInput';
import NumberInput from './NumberInput';
import OAuthInputConnector from './OAuthInputConnector';
import PasswordInput from './PasswordInput';
import PathInputConnector from './PathInputConnector';
import QualityProfileSelectInputConnector from './QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from './RootFolderSelectInputConnector';
import SeriesTagInput from './SeriesTagInput';
import SeriesTypeSelectInput from './SeriesTypeSelectInput';
import TagInputConnector from './TagInputConnector';
import TagSelectInputConnector from './TagSelectInputConnector';
import TextArea from './TextArea';
import TextInput from './TextInput';
import TextTagInputConnector from './TextTagInputConnector';
import UMaskInput from './UMaskInput';
import styles from './FormInputGroup.css';

function getComponent(type) {
  switch (type) {
    case inputTypes.AUTO_COMPLETE:
      return AutoCompleteInput;

    case inputTypes.CAPTCHA:
      return CaptchaInputConnector;

    case inputTypes.CHECK:
      return CheckInput;

    case inputTypes.DEVICE:
      return DeviceInputConnector;

    case inputTypes.KEY_VALUE_LIST:
      return KeyValueListInput;

    case inputTypes.MONITOR_EPISODES_SELECT:
      return MonitorEpisodesSelectInput;

    case inputTypes.MONITOR_NEW_ITEMS_SELECT:
      return MonitorNewItemsSelectInput;

    case inputTypes.NUMBER:
      return NumberInput;

    case inputTypes.OAUTH:
      return OAuthInputConnector;

    case inputTypes.PASSWORD:
      return PasswordInput;

    case inputTypes.PATH:
      return PathInputConnector;

    case inputTypes.QUALITY_PROFILE_SELECT:
      return QualityProfileSelectInputConnector;

    case inputTypes.INDEXER_SELECT:
      return IndexerSelectInputConnector;

    case inputTypes.INDEXER_FLAGS_SELECT:
      return IndexerFlagsSelectInput;

    case inputTypes.DOWNLOAD_CLIENT_SELECT:
      return DownloadClientSelectInputConnector;

    case inputTypes.ROOT_FOLDER_SELECT:
      return RootFolderSelectInputConnector;

    case inputTypes.SELECT:
      return EnhancedSelectInput;

    case inputTypes.DYNAMIC_SELECT:
      return EnhancedSelectInputConnector;

    case inputTypes.SERIES_TAG:
      return SeriesTagInput;

    case inputTypes.SERIES_TYPE_SELECT:
      return SeriesTypeSelectInput;

    case inputTypes.TAG:
      return TagInputConnector;

    case inputTypes.TEXT_AREA:
      return TextArea;

    case inputTypes.TEXT_TAG:
      return TextTagInputConnector;

    case inputTypes.TAG_SELECT:
      return TagSelectInputConnector;

    case inputTypes.UMASK:
      return UMaskInput;

    default:
      return TextInput;
  }
}

function FormInputGroup(props) {
  const {
    className,
    containerClassName,
    inputClassName,
    type,
    unit,
    buttons,
    helpText,
    helpTexts,
    helpTextWarning,
    helpLink,
    pending,
    errors,
    warnings,
    ...otherProps
  } = props;

  const InputComponent = getComponent(type);
  const checkInput = type === inputTypes.CHECK;
  const hasError = !!errors.length;
  const hasWarning = !hasError && !!warnings.length;
  const buttonsArray = React.Children.toArray(buttons);
  const lastButtonIndex = buttonsArray.length - 1;
  const hasButton = !!buttonsArray.length;

  return (
    <div className={containerClassName}>
      <div className={className}>
        <div className={styles.inputContainer}>
          <InputComponent
            className={inputClassName}
            helpText={helpText}
            helpTextWarning={helpTextWarning}
            hasError={hasError}
            hasWarning={hasWarning}
            hasButton={hasButton}
            {...otherProps}
          />

          {
            unit &&
              <div
                className={
                  type === inputTypes.NUMBER ?
                    styles.inputUnitNumber :
                    styles.inputUnit
                }
              >
                {unit}
              </div>
          }
        </div>

        {
          buttonsArray.map((button, index) => {
            return React.cloneElement(
              button,
              {
                isLastButton: index === lastButtonIndex
              }
            );
          })
        }

        {/* <div className={styles.pendingChangesContainer}>
          {
          pending &&
          <Icon
          name={icons.UNSAVED_SETTING}
          className={styles.pendingChangesIcon}
          title="Change has not been saved yet"
          />
          }
        </div> */}
      </div>

      {
        !checkInput && helpText &&
          <FormInputHelpText
            text={helpText}
          />
      }

      {
        !checkInput && helpTexts &&
          <div>
            {
              helpTexts.map((text, index) => {
                return (
                  <FormInputHelpText
                    key={index}
                    text={text}
                    isCheckInput={checkInput}
                  />
                );
              })
            }
          </div>
      }

      {
        (!checkInput || helpText) && helpTextWarning &&
          <FormInputHelpText
            text={helpTextWarning}
            isWarning={true}
          />
      }

      {
        helpLink &&
          <Link
            to={helpLink}
          >
            {translate('MoreInfo')}
          </Link>
      }

      {
        errors.map((error, index) => {
          return (
            <FormInputHelpText
              key={index}
              text={error.message}
              link={error.link}
              tooltip={error.detailedMessage}
              isError={true}
              isCheckInput={checkInput}
            />
          );
        })
      }

      {
        warnings.map((warning, index) => {
          return (
            <FormInputHelpText
              key={index}
              text={warning.message}
              link={warning.link}
              tooltip={warning.detailedMessage}
              isWarning={true}
              isCheckInput={checkInput}
            />
          );
        })
      }
    </div>
  );
}

FormInputGroup.propTypes = {
  className: PropTypes.string.isRequired,
  containerClassName: PropTypes.string.isRequired,
  inputClassName: PropTypes.string,
  name: PropTypes.string.isRequired,
  value: PropTypes.any,
  values: PropTypes.arrayOf(PropTypes.any),
  isDisabled: PropTypes.bool,
  type: PropTypes.string.isRequired,
  kind: PropTypes.oneOf(kinds.all),
  min: PropTypes.number,
  max: PropTypes.number,
  unit: PropTypes.string,
  buttons: PropTypes.oneOfType([PropTypes.node, PropTypes.arrayOf(PropTypes.node)]),
  helpText: PropTypes.string,
  helpTexts: PropTypes.arrayOf(PropTypes.string),
  helpTextWarning: PropTypes.string,
  helpLink: PropTypes.string,
  autoFocus: PropTypes.bool,
  includeNoChange: PropTypes.bool,
  includeNoChangeDisabled: PropTypes.bool,
  selectedValueOptions: PropTypes.object,
  indexerFlags: PropTypes.number,
  pending: PropTypes.bool,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  onChange: PropTypes.func.isRequired
};

FormInputGroup.defaultProps = {
  className: styles.inputGroup,
  containerClassName: styles.inputGroupContainer,
  type: inputTypes.TEXT,
  buttons: [],
  helpTexts: [],
  errors: [],
  warnings: []
};

export default FormInputGroup;
