import React, { ComponentType, FocusEvent, ReactNode } from 'react';
import Link from 'Components/Link/Link';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { inputTypes } from 'Helpers/Props';
import { InputType } from 'Helpers/Props/inputTypes';
import { Kind } from 'Helpers/Props/kinds';
import { InputChanged } from 'typings/inputs';
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

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const componentMap: Record<InputType, ComponentType<any>> = {
  autoComplete: AutoCompleteInput,
  captcha: CaptchaInput,
  check: CheckInput,
  date: TextInput,
  device: DeviceInput,
  file: TextInput,
  float: NumberInput,
  keyValueList: KeyValueListInput,
  languageSelect: LanguageSelectInput,
  monitorEpisodesSelect: MonitorEpisodesSelectInput,
  monitorNewItemsSelect: MonitorNewItemsSelectInput,
  number: NumberInput,
  oauth: OAuthInput,
  password: PasswordInput,
  path: PathInput,
  qualityProfileSelect: QualityProfileSelectInput,
  indexerSelect: IndexerSelectInput,
  indexerFlagsSelect: IndexerFlagsSelectInput,
  downloadClientSelect: DownloadClientSelectInput,
  rootFolderSelect: RootFolderSelectInput,
  select: EnhancedSelectInput,
  dynamicSelect: ProviderDataSelectInput,
  tag: SeriesTagInput,
  seriesTag: SeriesTagInput,
  seriesTypeSelect: SeriesTypeSelectInput,
  text: TextInput,
  textArea: TextArea,
  textTag: TextTagInput,
  tagSelect: TagSelectInput,
  umask: UMaskInput,
};

export interface FormInputGroupValues<T> {
  key: T;
  value: string;
  hint?: string;
}

// TODO: Remove once all parent components are updated to TSX and we can refactor to a consistent type
export interface ValidationMessage {
  message: string;
}

interface FormInputGroupProps<T> {
  className?: string;
  containerClassName?: string;
  inputClassName?: string;
  autocomplete?: string;
  name: string;
  value?: unknown;
  values?: FormInputGroupValues<unknown>[];
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
  includeFiles?: boolean;
  includeMissingValue?: boolean;
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  valueOptions?: object;
  selectedValueOptions?: object;
  selectOptionsProviderAction?: string;
  indexerFlags?: number;
  pending?: boolean;
  protocol?: DownloadProtocol;
  canEdit?: boolean;
  includeAny?: boolean;
  delimiters?: string[];
  readOnly?: boolean;
  errors?: (ValidationMessage | ValidationError)[];
  warnings?: (ValidationMessage | ValidationWarning)[];
  onChange: (change: InputChanged<T>) => void;
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

  const InputComponent = componentMap[type];
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
