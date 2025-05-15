import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { QualityProfilesAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useMeasure from 'Helpers/Hooks/useMeasure';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import {
  fetchQualityProfileSchema,
  saveQualityProfile,
  setQualityProfileValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import createQualityProfileInUseSelector from 'Store/Selectors/createQualityProfileInUseSelector';
import dimensions from 'Styles/Variables/dimensions';
import { InputChanged } from 'typings/inputs';
import QualityProfile, {
  QualityProfileGroup,
  QualityProfileQualityItem,
} from 'typings/QualityProfile';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItems from './QualityProfileFormatItems';
import { DragMoveState } from './QualityProfileItemDragSource';
import QualityProfileItems, {
  EditQualityProfileMode,
} from './QualityProfileItems';
import { SizeChanged } from './QualityProfileItemSize';
import styles from './EditQualityProfileModalContent.css';

const MODAL_BODY_PADDING = parseInt(dimensions.modalBodyPadding);

function parseIndex(index: string): [number | null, number] {
  const split = index.split('.');

  if (split.length === 1) {
    return [null, parseInt(split[0]) - 1];
  }

  return [parseInt(split[0]) - 1, parseInt(split[1]) - 1];
}

interface EditQualityProfileModalContentProps {
  id?: number;
  onContentHeightChange: (height: number) => void;
  onDeleteQualityProfilePress?: () => void;
  onModalClose: () => void;
}

function EditQualityProfileModalContent({
  id,
  onContentHeightChange,
  onDeleteQualityProfilePress,
  onModalClose,
}: EditQualityProfileModalContentProps) {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, isSaving, saveError, item } =
    useSelector(
      createProviderSettingsSelectorHook<
        QualityProfile,
        QualityProfilesAppState
      >('qualityProfiles', id)
    );

  const isInUse = useSelector(createQualityProfileInUseSelector(id));

  const [measureHeaderRef, { height: headerHeight }] = useMeasure();
  const [measureBodyRef, { height: bodyHeight }] = useMeasure();
  const [measureFooterRef, { height: footerHeight }] = useMeasure();

  const [mode, setMode] = useState<EditQualityProfileMode>('default');
  const [defaultBodyHeight, setDefaultBodyHeight] = useState(0);
  const [editGroupsBodyHeight, setEditGroupsBodyHeight] = useState(0);
  const [editSizesBodyHeight, setEditSizesBodyHeight] = useState(0);
  const [dndState, setDndState] = useState<DragMoveState>({
    dragQualityIndex: null,
    dropQualityIndex: null,
    dropPosition: null,
  });

  const wasSaving = usePrevious(isSaving);
  const { dragQualityIndex, dropQualityIndex, dropPosition } = dndState;

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
      // @ts-expect-error - actions are not typed
      dispatch(setQualityProfileValue({ name, value }));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveQualityProfile({ id }));
  }, [id, dispatch]);

  const handleCutoffChange = useCallback(
    ({ name, value }: InputChanged<number>) => {
      const cutoffItem = items.value.find((item) => {
        return 'id' in item ? item.id === value : item.quality.id === value;
      });

      if (cutoffItem) {
        const cutoffId =
          'id' in cutoffItem ? cutoffItem.id : cutoffItem.quality.id;

        // @ts-expect-error - actions are not typed
        dispatch(setQualityProfileValue({ name, value: cutoffId }));
      }
    },
    [items, dispatch]
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

      dispatch(
        // @ts-expect-error - actions are not typed
        setQualityProfileValue({
          name: 'items',
          value: newItems,
        })
      );
    },
    [items, dispatch]
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

      dispatch(
        // @ts-expect-error - actions are not typed
        setQualityProfileValue({
          name: 'items',
          value: newItems,
        })
      );
    },
    [items, dispatch]
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

      // @ts-expect-error - actions are not typed
      dispatch(setQualityProfileValue({ name: 'items', value: newItems }));
    },
    [items, dispatch]
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

      dispatch(
        // @ts-expect-error - actions are not typed
        setQualityProfileValue({
          name: 'items',
          value: newItems,
        })
      );
    },
    [items, dispatch]
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

      // @ts-expect-error - actions are not typed
      dispatch(setQualityProfileValue({ name: 'items', value: newItems }));
    },
    [items, dispatch]
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

      // @ts-expect-error - actions are not typed
      dispatch(setQualityProfileValue({ name: 'items', value: newItems }));
    },
    [items, dispatch]
  );

  const handleDragMove = useCallback((options: DragMoveState) => {
    const { dragQualityIndex, dropQualityIndex, dropPosition } = options;

    if (!dragQualityIndex || !dropQualityIndex || !dropPosition) {
      setDndState({
        dragQualityIndex: null,
        dropQualityIndex: null,
        dropPosition: null,
      });

      return;
    }

    const [dragGroupIndex, dragItemIndex] = parseIndex(dragQualityIndex);
    const [dropGroupIndex, dropItemIndex] = parseIndex(dropQualityIndex);

    if (
      (dropPosition === 'below' && dropItemIndex - 1 === dragItemIndex) ||
      (dropPosition === 'above' && dropItemIndex + 1 === dragItemIndex)
    ) {
      setDndState({
        dragQualityIndex: null,
        dropQualityIndex: null,
        dropPosition: null,
      });

      return;
    }

    let adjustedDropQualityIndex = dropQualityIndex;

    // Correct dragging out of a group to the position above
    if (
      dropPosition === 'above' &&
      dragGroupIndex !== dropGroupIndex &&
      dropGroupIndex != null
    ) {
      // Add 1 to the group index and 2 to the item index so it's inserted above in the correct group
      adjustedDropQualityIndex = `${dropGroupIndex + 1}.${dropItemIndex + 2}`;
    }

    // Correct inserting above outside a group
    if (
      dropPosition === 'above' &&
      dragGroupIndex !== dropGroupIndex &&
      dropGroupIndex == null
    ) {
      // Add 2 to the item index so it's entered in the correct place
      adjustedDropQualityIndex = `${dropItemIndex + 2}`;
    }

    // Correct inserting below a quality within the same group (when moving a lower item)
    if (
      dropPosition === 'below' &&
      dragGroupIndex === dropGroupIndex &&
      dropGroupIndex != null &&
      dragItemIndex < dropItemIndex
    ) {
      // Add 1 to the group index leave the item index
      adjustedDropQualityIndex = `${dropGroupIndex + 1}.${dropItemIndex}`;
    }

    // Correct inserting below a quality outside a group (when moving a lower item)
    if (
      dropPosition === 'below' &&
      dragGroupIndex === dropGroupIndex &&
      dropGroupIndex == null &&
      dragItemIndex < dropItemIndex
    ) {
      // Leave the item index so it's inserted below the item
      adjustedDropQualityIndex = `${dropItemIndex}`;
    }

    setDndState({
      dragQualityIndex,
      dropQualityIndex: adjustedDropQualityIndex,
      dropPosition,
    });
  }, []);

  const handleDragEnd = useCallback(
    (didDrop: boolean) => {
      if (didDrop && dragQualityIndex != null && dropQualityIndex != null) {
        const newItems = items.value.map((i) => {
          if ('id' in i) {
            return {
              ...i,
              items: [...i.items],
            } as QualityProfileGroup;
          }

          return {
            ...i,
          } as QualityProfileQualityItem;
        });

        const [dragGroupIndex, dragItemIndex] = parseIndex(dragQualityIndex);
        const [dropGroupIndex, dropItemIndex] = parseIndex(dropQualityIndex);

        let item: QualityProfileQualityItem | null = null;
        let dropGroup: QualityProfileGroup | null = null;

        // Get the group before moving anything so we know the correct place to drop it.
        if (dropGroupIndex != null) {
          dropGroup = newItems[dropGroupIndex] as QualityProfileGroup;
        }

        if (dragGroupIndex == null) {
          item = newItems.splice(
            dragItemIndex,
            1
          )[0] as QualityProfileQualityItem;
        } else {
          const group = newItems[dragGroupIndex] as QualityProfileGroup;

          item = group.items.splice(dragItemIndex, 1)[0];

          // If the group is now empty, destroy it.
          if (!group.items.length) {
            newItems.splice(dragGroupIndex, 1);
          }
        }

        if (dropGroup == null) {
          newItems.splice(dropItemIndex, 0, item);
        } else {
          dropGroup.items.splice(dropItemIndex, 0, item);
        }

        dispatch(
          // @ts-expect-error - actions are not typed
          setQualityProfileValue({
            name: 'items',
            value: newItems,
          })
        );
      }

      setDndState({
        dragQualityIndex: null,
        dropQualityIndex: null,
        dropPosition: null,
      });
    },
    [dragQualityIndex, dropQualityIndex, items, dispatch]
  );

  const handleChangeMode = useCallback((newMode: EditQualityProfileMode) => {
    setMode(newMode);
  }, []);

  const handleFormatItemScoreChange = useCallback(
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

      dispatch(
        // @ts-expect-error - actions are not typed
        setQualityProfileValue({
          name: 'formatItems',
          value: newFormatItems,
        })
      );
    },
    [formatItems, dispatch]
  );

  useEffect(() => {
    let bodyHeight = 0;

    if (mode === 'default') {
      bodyHeight = defaultBodyHeight;
    } else if (mode === 'editGroups') {
      bodyHeight = editGroupsBodyHeight;
    } else if (mode === 'editSizes') {
      bodyHeight = editSizesBodyHeight;
    }

    const padding = MODAL_BODY_PADDING * 2;

    onContentHeightChange(headerHeight + bodyHeight + footerHeight + padding);
  }, [
    headerHeight,
    defaultBodyHeight,
    editGroupsBodyHeight,
    editSizesBodyHeight,
    footerHeight,
    mode,
    onContentHeightChange,
  ]);

  useEffect(() => {
    if (mode === 'default') {
      setDefaultBodyHeight(bodyHeight);
    } else if (mode === 'editGroups') {
      setEditGroupsBodyHeight(bodyHeight);
    } else if (mode === 'editSizes') {
      setEditSizesBodyHeight(bodyHeight);
    }
  }, [bodyHeight, mode]);

  useEffect(() => {
    if (!id && !isPopulated) {
      dispatch(fetchQualityProfileSchema());
    }
  }, [id, isPopulated, dispatch]);

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

        // @ts-expect-error - actions are not typed
        dispatch(setQualityProfileValue({ name: 'cutoff', value: cutoffId }));
      }
    }
  }, [cutoff, items, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader ref={measureHeaderRef}>
        {id ? translate('EditQualityProfile') : translate('AddQualityProfile')}
      </ModalHeader>

      <ModalBody>
        <div ref={measureBodyRef}>
          {isPopulated ? null : <LoadingIndicator />}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('AddQualityProfileError')}
            </Alert>
          ) : null}

          {isPopulated && !error ? (
            <Form>
              <div className={styles.formGroupsContainer}>
                <div className={styles.formGroupWrapper}>
                  <FormGroup size={sizes.EXTRA_SMALL}>
                    <FormLabel size={sizes.SMALL}>
                      {translate('Name')}
                    </FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="name"
                      {...name}
                      onChange={handleInputChange}
                    />
                  </FormGroup>

                  <FormGroup size={sizes.EXTRA_SMALL}>
                    <FormLabel size={sizes.SMALL}>
                      {translate('UpgradesAllowed')}
                    </FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="upgradeAllowed"
                      {...upgradeAllowed}
                      helpText={translate('UpgradesAllowedHelpText')}
                      onChange={handleInputChange}
                    />
                  </FormGroup>

                  {upgradeAllowed.value ? (
                    <FormGroup size={sizes.EXTRA_SMALL}>
                      <FormLabel size={sizes.SMALL}>
                        {translate('UpgradeUntil')}
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.SELECT}
                        name="cutoff"
                        {...cutoff}
                        values={qualities}
                        helpText={translate('UpgradeUntilEpisodeHelpText')}
                        onChange={handleCutoffChange}
                      />
                    </FormGroup>
                  ) : null}

                  {formatItems.value.length > 0 ? (
                    <FormGroup size={sizes.EXTRA_SMALL}>
                      <FormLabel size={sizes.SMALL}>
                        {translate('MinimumCustomFormatScore')}
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.NUMBER}
                        name="minFormatScore"
                        {...minFormatScore}
                        helpText={translate('MinimumCustomFormatScoreHelpText')}
                        onChange={handleInputChange}
                      />
                    </FormGroup>
                  ) : null}

                  {upgradeAllowed.value && formatItems.value.length > 0 ? (
                    <FormGroup size={sizes.EXTRA_SMALL}>
                      <FormLabel size={sizes.SMALL}>
                        {translate('UpgradeUntilCustomFormatScore')}
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.NUMBER}
                        name="cutoffFormatScore"
                        {...cutoffFormatScore}
                        helpText={translate(
                          'UpgradeUntilCustomFormatScoreEpisodeHelpText'
                        )}
                        onChange={handleInputChange}
                      />
                    </FormGroup>
                  ) : null}

                  {upgradeAllowed.value && formatItems.value.length > 0 ? (
                    <FormGroup size={sizes.EXTRA_SMALL}>
                      <FormLabel size={sizes.SMALL}>
                        {translate('MinimumCustomFormatScoreIncrement')}
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.NUMBER}
                        name="minUpgradeFormatScore"
                        min={1}
                        {...minUpgradeFormatScore}
                        helpText={translate(
                          'MinimumCustomFormatScoreIncrementHelpText'
                        )}
                        onChange={handleInputChange}
                      />
                    </FormGroup>
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

                <div className={styles.formGroupWrapper}>
                  <QualityProfileItems
                    mode={mode}
                    qualityProfileItems={items.value}
                    errors={items.errors}
                    warnings={items.warnings}
                    dragQualityIndex={dragQualityIndex}
                    dropQualityIndex={dropQualityIndex}
                    dropPosition={dropPosition}
                    onChangeMode={handleChangeMode}
                    onCreateGroupPress={handleCreateGroupPress}
                    onDeleteGroupPress={handleDeleteGroupPress}
                    onItemAllowedChange={handleItemAllowedChange}
                    onGroupAllowedChange={handleGroupAllowedChange}
                    onItemGroupNameChange={handleGroupNameChange}
                    onDragMove={handleDragMove}
                    onDragEnd={handleDragEnd}
                    onSizeChange={handleSizeChange}
                  />
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
        </div>
      </ModalBody>

      <ModalFooter ref={measureFooterRef}>
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
