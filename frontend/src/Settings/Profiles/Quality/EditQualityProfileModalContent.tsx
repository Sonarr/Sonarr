import { DragDropProvider } from '@dnd-kit/react';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDebouncedCallback } from 'use-debounce';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import useQualityProfileInUse from 'Settings/Profiles/Quality/useQualityProfileInUse';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItems from './QualityProfileFormatItems';
import {
  mapFailuresByQualityId,
  parseItemFailures,
} from './qualityProfileItemFailures';
import QualityProfileItems, {
  EditQualityProfileMode,
} from './QualityProfileItems';
import { SizeChanged } from './QualityProfileItemSize';
import useQualityProfileDnd from './useQualityProfileDnd';
import {
  QualityProfileGroup,
  QualityProfileItems as QualityProfileItemsType,
  QualityProfileQualityItem,
  useManageQualityProfile,
} from './useQualityProfiles';
import styles from './EditQualityProfileModalContent.css';

interface EditQualityProfileModalContentProps {
  id?: number;
  cloneId?: number;
  onDeleteQualityProfilePress?: () => void;
  onModalClose: () => void;
}

function EditQualityProfileModalContent({
  id,
  cloneId,
  onDeleteQualityProfilePress,
  onModalClose,
}: EditQualityProfileModalContentProps) {
  const {
    item,
    isSaving,
    saveError,
    isSchemaLoading,
    isSchemaFetched,
    schemaError,
    updateValue,
    saveProvider,
    validationErrors,
    validationWarnings,
  } = useManageQualityProfile(id, cloneId);

  const itemFailures = useMemo(
    () => parseItemFailures(validationErrors, validationWarnings),
    [validationErrors, validationWarnings]
  );

  const failuresByQualityId = useMemo(
    () => mapFailuresByQualityId(item.items?.value ?? [], itemFailures),
    [item.items, itemFailures]
  );

  const { seriesCount, importListCount } = useQualityProfileInUse(id);
  const isInUse = seriesCount !== 0 || importListCount !== 0;

  const [mode, setMode] = useState<EditQualityProfileMode>('default');

  const wasSaving = usePrevious(isSaving);

  const {
    name,
    upgradeAllowed,
    cutoff,
    minFormatScore,
    minUpgradeFormatScore,
    cutoffFormatScore,
    items,
    formatItems,
  } = item;

  const qualities = useMemo(() => {
    if (!items?.value) {
      return [];
    }

    return items.value.reduceRight<{ key: number; value: string }[]>(
      (acc, item) => {
        if (item.allowed) {
          if ('id' in item) {
            acc.push({
              key: item.id,
              value: item.name,
            });
          } else {
            acc.push({
              key: item.quality.id,
              value: item.quality.name,
            });
          }
        }

        return acc;
      },
      []
    );
  }, [items]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - change is not yet typed
      updateValue(name, value);
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  const handleCutoffChange = useCallback(
    ({ name, value }: InputChanged<number>) => {
      const cutoffItem = items.value.find((item) => {
        return 'id' in item ? item.id === value : item.quality.id === value;
      });

      if (cutoffItem) {
        const cutoffId =
          'id' in cutoffItem ? cutoffItem.id : cutoffItem.quality.id;

        // @ts-expect-error - actions are not typed
        updateValue(name, cutoffId);
      }
    },
    [items, updateValue]
  );

  const handleItemAllowedChange = useCallback(
    (qualityId: number, allowed: boolean) => {
      const newItems = items.value.map((item) => {
        if ('quality' in item && item.quality.id === qualityId) {
          return {
            ...item,
            allowed,
          };
        }

        return item;
      });

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleGroupAllowedChange = useCallback(
    (groupId: number, allowed: boolean) => {
      const newItems = items.value.map((item) => {
        if ('id' in item && item.id === groupId) {
          return {
            ...item,
            allowed,
          };
        }

        return item;
      });

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleGroupNameChange = useCallback(
    (groupId: number, name: string) => {
      const newItems = items.value.map((item) => {
        if ('id' in item && item.id === groupId) {
          return {
            ...item,
            name,
          };
        }

        return item;
      });

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleSizeChange = useCallback(
    (sizeChange: SizeChanged) => {
      const { qualityId, ...sizes } = sizeChange;

      const newItems = items.value.map((item) => {
        if ('quality' in item && item.quality.id === qualityId) {
          return {
            ...item,
            ...sizes,
          };
        }

        return {
          ...item,
          items: (item as QualityProfileGroup).items.map((subItem) => {
            if (subItem.quality.id === qualityId) {
              return {
                ...subItem,
                ...sizes,
              };
            }

            return subItem;
          }),
        };
      });

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleCreateGroupPress = useCallback(
    (qualityId: number) => {
      const groupId =
        items.value.reduce((acc, item) => {
          if ('id' in item && item.id > acc) {
            acc = item.id;
          }

          return acc;
        }, 1000) + 1;

      const newItems = items.value.map((item) => {
        if ('quality' in item && item.quality.id === qualityId) {
          return {
            id: groupId,
            name: item.quality.name,
            allowed: item.allowed,
            items: [item],
          };
        }

        return item;
      });

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleDeleteGroupPress = useCallback(
    (groupId: number) => {
      const newItems = items.value.reduce<QualityProfileQualityItem[]>(
        (acc, item) => {
          if ('id' in item && item.id === groupId) {
            acc.push(...item.items);
          } else {
            acc.push(item as QualityProfileQualityItem);
          }

          return acc;
        },
        []
      );

      updateValue('items', newItems);
    },
    [items, updateValue]
  );

  const handleItemsChange = useCallback(
    (newItems: QualityProfileItemsType) => {
      updateValue('items', newItems);
    },
    [updateValue]
  );

  const { displayItems, handleDragStart, handleDragOver, handleDragEnd } =
    useQualityProfileDnd(items?.value ?? [], handleItemsChange);

  const handleChangeMode = useCallback((newMode: EditQualityProfileMode) => {
    setMode(newMode);
  }, []);

  const handleFormatItemScoreChange = useDebouncedCallback(
    (formatId: number, score: number) => {
      const newFormatItems = formatItems.value.map((formatItem) => {
        if (formatItem.format === formatId) {
          return {
            ...formatItem,
            score,
          };
        }

        return formatItem;
      });

      updateValue('formatItems', newFormatItems);
    },
    1000
  );

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  useEffect(() => {
    if (!items?.value) {
      return;
    }

    const cutoffItem = items.value.find((item) =>
      'id' in item ? item.id === cutoff.value : item.quality.id === cutoff.value
    );

    // If the cutoff isn't allowed anymore or there isn't a cutoff set one
    if (!cutoff || !cutoffItem || !cutoffItem.allowed) {
      const firstAllowed = items.value.find((item) => item.allowed);

      let cutoffId = null;

      if (firstAllowed) {
        cutoffId =
          'id' in firstAllowed ? firstAllowed.id : firstAllowed.quality.id;

        updateValue('cutoff', cutoffId);
      }
    }
  }, [cutoff, items, updateValue]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditQualityProfile') : translate('AddQualityProfile')}
      </ModalHeader>

      <ModalBody>
        {isSchemaFetched ? null : <LoadingIndicator />}

        {!isSchemaLoading && schemaError ? (
          <Alert kind={kinds.DANGER}>
            {translate('AddQualityProfileError')}
          </Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <Form>
            <div className={styles.formGroupsContainer}>
              <div className={styles.formGroupWrapper}>
                <FormRow>
                  <FormLabel>{translate('Name')}</FormLabel>

                  <FormInput
                    type={inputTypes.TEXT}
                    name="name"
                    {...name}
                    onChange={handleInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('UpgradesAllowed')}</FormLabel>

                  <FormInputHelpText
                    text={translate('UpgradesAllowedHelpText')}
                  />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="upgradeAllowed"
                    {...upgradeAllowed}
                    onChange={handleInputChange}
                  />
                </FormRow>

                {upgradeAllowed.value ? (
                  <FormRow>
                    <FormLabel>{translate('UpgradeUntil')}</FormLabel>

                    <FormInputHelpText
                      text={translate('UpgradeUntilEpisodeHelpText')}
                    />

                    <FormInput
                      type={inputTypes.SELECT}
                      name="cutoff"
                      {...cutoff}
                      values={qualities}
                      onChange={handleCutoffChange}
                    />
                  </FormRow>
                ) : null}

                {formatItems.value.length > 0 ? (
                  <FormRow>
                    <FormLabel>
                      {translate('MinimumCustomFormatScore')}
                    </FormLabel>

                    <FormInputHelpText
                      text={translate('MinimumCustomFormatScoreHelpText')}
                    />

                    <FormInput
                      type={inputTypes.NUMBER}
                      name="minFormatScore"
                      {...minFormatScore}
                      onChange={handleInputChange}
                    />
                  </FormRow>
                ) : null}

                {upgradeAllowed.value && formatItems.value.length > 0 ? (
                  <FormRow>
                    <FormLabel>
                      {translate('UpgradeUntilCustomFormatScore')}
                    </FormLabel>

                    <FormInputHelpText
                      text={translate(
                        'UpgradeUntilCustomFormatScoreEpisodeHelpText'
                      )}
                    />

                    <FormInput
                      type={inputTypes.NUMBER}
                      name="cutoffFormatScore"
                      {...cutoffFormatScore}
                      onChange={handleInputChange}
                    />
                  </FormRow>
                ) : null}

                {upgradeAllowed.value && formatItems.value.length > 0 ? (
                  <FormRow>
                    <FormLabel>
                      {translate('MinimumCustomFormatScoreIncrement')}
                    </FormLabel>

                    <FormInputHelpText
                      text={translate(
                        'MinimumCustomFormatScoreIncrementHelpText'
                      )}
                    />

                    <FormInput
                      type={inputTypes.NUMBER}
                      name="minUpgradeFormatScore"
                      min={1}
                      {...minUpgradeFormatScore}
                      onChange={handleInputChange}
                    />
                  </FormRow>
                ) : null}

                <div className={styles.formatItemLarge}>
                  <QualityProfileFormatItems
                    profileFormatItems={formatItems.value}
                    errors={formatItems.errors}
                    warnings={formatItems.warnings}
                    onQualityProfileFormatItemScoreChange={
                      handleFormatItemScoreChange
                    }
                  />
                </div>
              </div>

              <div className={styles.qualitiesColumn}>
                <DragDropProvider
                  onDragStart={handleDragStart}
                  onDragOver={handleDragOver}
                  onDragEnd={handleDragEnd}
                >
                  <QualityProfileItems
                    mode={mode}
                    displayItems={displayItems}
                    errors={items.errors}
                    warnings={items.warnings}
                    failuresByQualityId={failuresByQualityId}
                    onChangeMode={handleChangeMode}
                    onCreateGroupPress={handleCreateGroupPress}
                    onDeleteGroupPress={handleDeleteGroupPress}
                    onItemAllowedChange={handleItemAllowedChange}
                    onGroupAllowedChange={handleGroupAllowedChange}
                    onItemGroupNameChange={handleGroupNameChange}
                    onSizeChange={handleSizeChange}
                  />
                </DragDropProvider>
              </div>

              <div className={styles.formatItemSmall}>
                <QualityProfileFormatItems
                  profileFormatItems={formatItems.value}
                  errors={formatItems.errors}
                  warnings={formatItems.warnings}
                  onQualityProfileFormatItemScoreChange={
                    handleFormatItemScoreChange
                  }
                />
              </div>
            </div>
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {id ? (
          <div
            className={styles.deleteButtonContainer}
            title={
              isInUse
                ? translate('QualityProfileInUseSeriesListCollection')
                : undefined
            }
          >
            <Button
              kind={kinds.DANGER}
              isDisabled={isInUse}
              onPress={onDeleteQualityProfilePress}
            >
              {translate('Delete')}
            </Button>

            {isInUse ? (
              <Popover
                title={translate('QualityProfileUsage')}
                body={
                  <div>
                    {seriesCount ? (
                      <div>
                        {translate('QualityProfileUsedInCountSeries', {
                          count: seriesCount,
                        })}
                      </div>
                    ) : null}
                    {importListCount ? (
                      <div>
                        {translate('QualityProfileUsedInCountImportLists', {
                          count: importListCount,
                        })}
                      </div>
                    ) : null}
                  </div>
                }
                anchor={
                  <Icon
                    className={styles.deleteButtonInfoIcon}
                    name={icons.INFO}
                  />
                }
              />
            ) : null}
          </div>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditQualityProfileModalContent;
