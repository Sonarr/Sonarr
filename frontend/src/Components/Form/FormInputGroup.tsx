import React, { ElementType, ReactNode } from 'react';
import Link from 'Components/Link/Link';
import { inputTypes } from 'Helpers/Props';
import { InputType } from 'Helpers/Props/inputTypes';
import { ValidationError, ValidationWarning } from 'typings/pending';
import translate from 'Utilities/String/translate';
import AutoCompleteInput, { AutoCompleteInputProps } from './AutoCompleteInput';
import CaptchaInput, { CaptchaInputProps } from './CaptchaInput';
import CheckInput, { CheckInputProps } from './CheckInput';
import FloatInput, { FloatInputProps } from './FloatInput';
import { FormInputButtonProps } from './FormInputButton';
import FormInputHelpText from './FormInputHelpText';
import KeyValueListInput, { KeyValueListInputProps } from './KeyValueListInput';
import NumberInput, { NumberInputProps } from './NumberInput';
import OAuthInput, { OAuthInputProps } from './OAuthInput';
import PasswordInput from './PasswordInput';
import PathInput, { PathInputProps } from './PathInput';
import DownloadClientSelectInput, {
  DownloadClientSelectInputProps,
} from './Select/DownloadClientSelectInput';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
} from './Select/EnhancedSelectInput';
import IndexerFlagsSelectInput, {
  IndexerFlagsSelectInputProps,
} from './Select/IndexerFlagsSelectInput';
import IndexerSelectInput, {
  IndexerSelectInputProps,
} from './Select/IndexerSelectInput';
import LanguageSelectInput, {
  LanguageSelectInputProps,
} from './Select/LanguageSelectInput';
import MonitorEpisodesSelectInput, {
  MonitorEpisodesSelectInputProps,
} from './Select/MonitorEpisodesSelectInput';
import MonitorNewItemsSelectInput, {
  MonitorNewItemsSelectInputProps,
} from './Select/MonitorNewItemsSelectInput';
import ProviderDataSelectInput, {
  ProviderOptionSelectInputProps,
} from './Select/ProviderOptionSelectInput';
import QualityProfileSelectInput, {
  QualityProfileSelectInputProps,
} from './Select/QualityProfileSelectInput';
import RootFolderSelectInput, {
  RootFolderSelectInputProps,
} from './Select/RootFolderSelectInput';
import SeriesTypeSelectInput, {
  SeriesTypeSelectInputProps,
} from './Select/SeriesTypeSelectInput';
import UMaskInput, { UMaskInputProps } from './Select/UMaskInput';
import DeviceInput, { DeviceInputProps } from './Tag/DeviceInput';
import SeriesTagInput, { SeriesTagInputProps } from './Tag/SeriesTagInput';
import TagSelectInput, { TagSelectInputProps } from './Tag/TagSelectInput';
import TextTagInput, { TextTagInputProps } from './Tag/TextTagInput';
import TextArea, { TextAreaProps } from './TextArea';
import TextInput, { TextInputProps } from './TextInput';
import styles from './FormInputGroup.css';

const componentMap: Record<InputType, ElementType> = {
  autoComplete: AutoCompleteInput,
  captcha: CaptchaInput,
  check: CheckInput,
  date: TextInput,
  device: DeviceInput,
  downloadClientSelect: DownloadClientSelectInput,
  dynamicSelect: ProviderDataSelectInput,
  file: TextInput,
  float: FloatInput,
  indexerFlagsSelect: IndexerFlagsSelectInput,
  indexerSelect: IndexerSelectInput,
  keyValueList: KeyValueListInput,
  languageSelect: LanguageSelectInput,
  monitorEpisodesSelect: MonitorEpisodesSelectInput,
  monitorNewItemsSelect: MonitorNewItemsSelectInput,
  number: NumberInput,
  oauth: OAuthInput,
  password: PasswordInput,
  path: PathInput,
  qualityProfileSelect: QualityProfileSelectInput,
  rootFolderSelect: RootFolderSelectInput,
  select: EnhancedSelectInput,
  seriesTag: SeriesTagInput,
  seriesTypeSelect: SeriesTypeSelectInput,
  tag: SeriesTagInput,
  tagSelect: TagSelectInput,
  text: TextInput,
  textArea: TextArea,
  textTag: TextTagInput,
  umask: UMaskInput,
} as const;

// type Components = typeof componentMap;

type PickProps<V, C extends InputType> = C extends 'text'
  ? TextInputProps
  : C extends 'autoComplete'
  ? AutoCompleteInputProps
  : C extends 'captcha'
  ? CaptchaInputProps
  : C extends 'check'
  ? CheckInputProps
  : C extends 'date'
  ? TextInputProps
  : C extends 'device'
  ? DeviceInputProps
  : C extends 'downloadClientSelect'
  ? DownloadClientSelectInputProps
  : C extends 'dynamicSelect'
  ? ProviderOptionSelectInputProps
  : C extends 'file'
  ? TextInputProps
  : C extends 'float'
  ? FloatInputProps
  : C extends 'indexerFlagsSelect'
  ? IndexerFlagsSelectInputProps
  : C extends 'indexerSelect'
  ? IndexerSelectInputProps
  : C extends 'keyValueList'
  ? KeyValueListInputProps
  : C extends 'languageSelect'
  ? LanguageSelectInputProps
  : C extends 'monitorEpisodesSelect'
  ? MonitorEpisodesSelectInputProps
  : C extends 'monitorNewItemsSelect'
  ? MonitorNewItemsSelectInputProps
  : C extends 'number'
  ? NumberInputProps
  : C extends 'oauth'
  ? OAuthInputProps
  : C extends 'password'
  ? TextInputProps
  : C extends 'path'
  ? PathInputProps
  : C extends 'qualityProfileSelect'
  ? QualityProfileSelectInputProps
  : C extends 'rootFolderSelect'
  ? RootFolderSelectInputProps
  : C extends 'select'
  ? // eslint-disable-next-line @typescript-eslint/no-explicit-any
    EnhancedSelectInputProps<any, V>
  : C extends 'seriesTag'
  ? SeriesTagInputProps<V>
  : C extends 'seriesTypeSelect'
  ? SeriesTypeSelectInputProps
  : C extends 'tag'
  ? SeriesTagInputProps<V>
  : C extends 'tagSelect'
  ? TagSelectInputProps
  : C extends 'text'
  ? TextInputProps
  : C extends 'textArea'
  ? TextAreaProps
  : C extends 'textTag'
  ? TextTagInputProps
  : C extends 'umask'
  ? UMaskInputProps
  : never;

export interface FormInputGroupValues<T> {
  key: T;
  value: string;
  hint?: string;
}

// TODO: Remove once all parent components are updated to TSX and we can refactor to a consistent type
export interface ValidationMessage {
  message: string;
}

export type FormInputGroupProps<V, C extends InputType> = Omit<
  PickProps<V, C>,
  'className'
> & {
  type: C;
  className?: string;
  containerClassName?: string;
  inputClassName?: string;
  autoFocus?: boolean;
  autocomplete?: string;
  name: string;
  buttons?: ReactNode | ReactNode[];
  helpText?: string;
  helpTexts?: string[];
  helpTextWarning?: string;
  helpLink?: string;
  pending?: boolean;
  placeholder?: string;
  unit?: string;
  errors?: (ValidationMessage | ValidationError)[];
  warnings?: (ValidationMessage | ValidationWarning)[];
};

function FormInputGroup<T, C extends InputType>(
  props: FormInputGroupProps<T, C>
) {
  const {
    className = styles.inputGroup,
    containerClassName = styles.inputGroupContainer,
    inputClassName,
    type,
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
          {/* @ts-expect-error - types are validated already */}
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
