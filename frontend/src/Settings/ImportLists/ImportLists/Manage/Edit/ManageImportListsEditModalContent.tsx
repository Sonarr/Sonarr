import React, { useCallback, useState } from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { inputTypes } from 'Helpers/Props';
import Language from 'Language/Language';
import tagExistingOptions from 'Settings/ImportLists/ImportLists/tagExistingOptions';
import { TagExisting } from 'Settings/ImportLists/ImportLists/useImportLists';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ManageImportListsEditModalContent.module.css';

interface SavePayload {
  enableAutomaticAdd?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  language?: Language;
  tagExisting?: TagExisting;
}

interface ManageImportListsEditModalContentProps {
  importListIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const tagExistingWithNoChangeOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  ...tagExistingOptions,
];

const autoAddOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

function ManageImportListsEditModalContent(
  props: ManageImportListsEditModalContentProps
) {
  const { importListIds, onSavePress, onModalClose } = props;

  const [enableAutomaticAdd, setEnableAutomaticAdd] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [language, setLanguage] = useState<string | Language>(NO_CHANGE);
  const [tagExisting, setTagExisting] = useState<
    TagExisting | typeof NO_CHANGE
  >(NO_CHANGE);

  const save = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (enableAutomaticAdd !== NO_CHANGE) {
      hasChanges = true;
      payload.enableAutomaticAdd = enableAutomaticAdd === 'enabled';
    }

    if (qualityProfileId !== NO_CHANGE) {
      hasChanges = true;
      payload.qualityProfileId = qualityProfileId as number;
    }

    if (rootFolderPath !== NO_CHANGE) {
      hasChanges = true;
      payload.rootFolderPath = rootFolderPath;
    }

    if (language !== NO_CHANGE) {
      hasChanges = true;
      payload.language = language as Language;
    }

    if (tagExisting !== NO_CHANGE) {
      hasChanges = true;
      payload.tagExisting = tagExisting;
    }

    if (hasChanges) {
      onSavePress(payload);
    }

    onModalClose();
  }, [
    enableAutomaticAdd,
    qualityProfileId,
    rootFolderPath,
    language,
    tagExisting,
    onSavePress,
    onModalClose,
  ]);

  const onInputChange = useCallback(({ name, value }: InputChanged) => {
    switch (name) {
      case 'enableAutomaticAdd':
        setEnableAutomaticAdd(value as string);
        break;
      case 'qualityProfileId':
        setQualityProfileId(value as number);
        break;
      case 'rootFolderPath':
        setRootFolderPath(value as string);
        break;
      case 'language':
        setLanguage(value as Language);
        break;
      case 'tagExisting':
        setTagExisting(value as TagExisting);
        break;
      default:
        console.warn(`EditImportListModalContent Unknown Input: '${name}'`);
    }
  }, []);

  const selectedCount = importListIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedImportLists')}</ModalHeader>
      <ModalBody>
        <FormRow>
          <FormLabel>{translate('AutomaticAdd')}</FormLabel>

          <FormInput
            type={inputTypes.SELECT}
            name="enableAutomaticAdd"
            value={enableAutomaticAdd}
            values={autoAddOptions}
            onChange={onInputChange}
          />
        </FormRow>

        <ModalSection
          title={translate('ManageImportListsEditModalContentLibrarySection')}
        >
          <FormRow>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInput
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              value={qualityProfileId}
              includeNoChange={true}
              includeNoChangeDisabled={false}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('RootFolder')}</FormLabel>

            <FormInput
              type={inputTypes.ROOT_FOLDER_SELECT}
              name="rootFolderPath"
              value={rootFolderPath}
              includeNoChange={true}
              includeNoChangeDisabled={false}
              selectedValueOptions={{ includeFreeSpace: false }}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Language')}</FormLabel>

            <FormInputHelpText text={translate('ListLanguageHelpText')} />
            <FormInput
              type={inputTypes.LANGUAGE_SELECT}
              name="language"
              value={language}
              includeAny={false}
              includeNoChange={true}
              includeNoChangeDisabled={false}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('TagExisting')}</FormLabel>
            <FormInput
              type={inputTypes.SELECT}
              name="tagExisting"
              value={tagExisting}
              values={tagExistingWithNoChangeOptions}
              onChange={onInputChange}
            />
          </FormRow>
        </ModalSection>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountImportListsSelected', {
            count: selectedCount,
          })}
        </div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={save}>{translate('ApplyChanges')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManageImportListsEditModalContent;
