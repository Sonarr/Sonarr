import React, { FocusEvent, ReactNode } from 'react';
import Link from 'Components/Link/Link';
import { inputTypes } from 'Helpers/Props';
import { InputType } from 'Helpers/Props/inputTypes';
import { Kind } from 'Helpers/Props/kinds';
import { ValidationError, ValidationWarning } from 'typings/pending';
import translate from 'Utilities/String/translate';
import AutoCompleteInput from './AutoCompleteInput';
import CaptchaInput from './CaptchaInput';
import CheckInput from './CheckInput';
import { FormInputButtonProps } from './FormInputButton';
import FormInputHelpText from './FormInputHelpText';
import KeyValueListInput from './KeyValueListInput';
import NumberInput from './NumberInput';
import OAuthInput from './OAuthInput';
import PasswordInput from './PasswordInput';
import PathInput from './PathInput';
import DownloadClientSelectInput from './Select/DownloadClientSelectInput';
import EnhancedSelectInput from './Select/EnhancedSelectInput';
import IndexerFlagsSelectInput from './Select/IndexerFlagsSelectInput';
import IndexerSelectInput from './Select/IndexerSelectInput';
import LanguageSelectInput from './Select/LanguageSelectInput';
import MonitorEpisodesSelectInput from './Select/MonitorEpisodesSelectInput';
import MonitorNewItemsSelectInput from './Select/MonitorNewItemsSelectInput';
import ProviderDataSelectInput from './Select/ProviderOptionSelectInput';
import QualityProfileSelectInput from './Select/QualityProfileSelectInput';
import RootFolderSelectInput from './Select/RootFolderSelectInput';
import SeriesTypeSelectInput from './Select/SeriesTypeSelectInput';
import UMaskInput from './Select/UMaskInput';
import DeviceInput from './Tag/DeviceInput';
import SeriesTagInput from './Tag/SeriesTagInput';
import TagSelectInput from './Tag/TagSelectInput';
import TextTagInput from './Tag/TextTagInput';
import TextArea from './TextArea';
import TextInput from './TextInput';
import styles from './FormInputGroup.css';

function getComponent(type: InputType) {
  switch (type) {
    case inputTypes.AUTO_COMPLETE:
      return AutoCompleteInput;

    case inputTypes.CAPTCHA:
      return CaptchaInput;

    case inputTypes.CHECK:
      return CheckInput;

    case inputTypes.DEVICE:
      return DeviceInput;

    case inputTypes.KEY_VALUE_LIST:
      return KeyValueListInput;

    case inputTypes.LANGUAGE_SELECT:
      return LanguageSelectInput;

    case inputTypes.MONITOR_EPISODES_SELECT:
      return MonitorEpisodesSelectInput;

    case inputTypes.MONITOR_NEW_ITEMS_SELECT:
      return MonitorNewItemsSelectInput;

    case inputTypes.NUMBER:
      return NumberInput;

    case inputTypes.OAUTH:
      return OAuthInput;

    case inputTypes.PASSWORD:
      return PasswordInput;

    case inputTypes.PATH:
      return PathInput;

    case inputTypes.QUALITY_PROFILE_SELECT:
      return QualityProfileSelectInput;

    case inputTypes.INDEXER_SELECT:
      return IndexerSelectInput;

    case inputTypes.INDEXER_FLAGS_SELECT:
      return IndexerFlagsSelectInput;

    case inputTypes.DOWNLOAD_CLIENT_SELECT:
      return DownloadClientSelectInput;

    case inputTypes.ROOT_FOLDER_SELECT:
      return RootFolderSelectInput;

    case inputTypes.SELECT:
      return EnhancedSelectInput;

    case inputTypes.DYNAMIC_SELECT:
      return ProviderDataSelectInput;

    case inputTypes.TAG:
    case inputTypes.SERIES_TAG:
      return SeriesTagInput;

    case inputTypes.SERIES_TYPE_SELECT:
      return SeriesTypeSelectInput;

    case inputTypes.TEXT_AREA:
      return TextArea;

    case inputTypes.TEXT_TAG:
      return TextTagInput;

    case inputTypes.TAG_SELECT:
      return TagSelectInput;

    case inputTypes.UMASK:
      return UMaskInput;

    default:
      return TextInput;
  }
}

// TODO: Remove once all parent components are updated to TSX and we can refactor to a consistent type
interface ValidationMessage {
  message: string;
}

interface FormInputGroupProps<T> {
  className?: string;
  containerClassName?: string;
  inputClassName?: string;
  name: string;
  value?: unknown;
  values?: unknown[];
  isDisabled?: boolean;
  type?: InputType;
  kind?: Kind;
  min?: number;
  max?: number;
  unit?: string;
  buttons?: ReactNode | ReactNode[];
  helpText?: string;
  helpTexts?: string[];
  helpTextWarning?: string;
  helpLink?: string;
  placeholder?: string;
  autoFocus?: boolean;
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  valueOptions?: object;
  selectedValueOptions?: object;
  indexerFlags?: number;
  pending?: boolean;
  canEdit?: boolean;
  includeAny?: boolean;
  delimiters?: string[];
  readOnly?: boolean;
  errors?: (ValidationMessage | ValidationError)[];
  warnings?: (ValidationMessage | ValidationWarning)[];
  onChange: (args: T) => void;
  onFocus?: (event: FocusEvent<HTMLInputElement>) => void;
}

function FormInputGroup<T>(props: FormInputGroupProps<T>) {
  const {
    className = styles.inputGroup,
    containerClassName = styles.inputGroupContainer,
    inputClassName,
    type = 'text',
    unit,
    buttons = [],
    helpText,
    helpTexts = [],
    helpTextWarning,
    helpLink,
    pending,
    errors = [],
    warnings = [],
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
          {/* @ts-expect-error - need to pass through all the expected options */}
          <InputComponent
            className={inputClassName}
            helpText={helpText}
            helpTextWarning={helpTextWarning}
            hasError={hasError}
            hasWarning={hasWarning}
            hasButton={hasButton}
            {...otherProps}
          />

          {unit && (
            <div
              className={
                type === inputTypes.NUMBER
                  ? styles.inputUnitNumber
                  : styles.inputUnit
              }
            >
              {unit}
            </div>
          )}
        </div>

        {buttonsArray.map((button, index) => {
          if (!React.isValidElement<FormInputButtonProps>(button)) {
            return button;
          }

          return React.cloneElement(button, {
            isLastButton: index === lastButtonIndex,
          });
        })}

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

      {!checkInput && helpText ? <FormInputHelpText text={helpText} /> : null}

      {!checkInput && helpTexts ? (
        <div>
          {helpTexts.map((text, index) => {
            return (
              <FormInputHelpText
                key={index}
                text={text}
                isCheckInput={checkInput}
              />
            );
          })}
        </div>
      ) : null}

      {(!checkInput || helpText) && helpTextWarning ? (
        <FormInputHelpText text={helpTextWarning} isWarning={true} />
      ) : null}

      {helpLink ? <Link to={helpLink}>{translate('MoreInfo')}</Link> : null}

      {errors.map((error, index) => {
        return 'errorMessage' in error ? (
          <FormInputHelpText
            key={index}
            text={error.errorMessage}
            link={error.infoLink}
            tooltip={error.detailedDescription}
            isError={true}
            isCheckInput={checkInput}
          />
        ) : (
          <FormInputHelpText
            key={index}
            text={error.message}
            isError={true}
            isCheckInput={checkInput}
          />
        );
      })}

      {warnings.map((warning, index) => {
        return 'errorMessage' in warning ? (
          <FormInputHelpText
            key={index}
            text={warning.errorMessage}
            link={warning.infoLink}
            tooltip={warning.detailedDescription}
            isWarning={true}
            isCheckInput={checkInput}
          />
        ) : (
          <FormInputHelpText
            key={index}
            text={warning.message}
            isWarning={true}
            isCheckInput={checkInput}
          />
        );
      })}
    </div>
  );
}

export default FormInputGroup;
