import React, { useCallback, useEffect, useMemo, useState } from 'react';
import useMeasure from 'react-use-measure';
import FormGroup from 'Components/Form/FormGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import { Failure } from 'typings/pending';
import { QualityProfileItems as Items } from 'typings/QualityProfile';
import translate from 'Utilities/String/translate';
import QualityProfileItemDragSource, {
  QualityProfileItemDragSourceActionProps,
} from './QualityProfileItemDragSource';
import styles from './QualityProfileItems.css';

export type EditQualityProfileMode = 'default' | 'editGroups' | 'editSizes';

interface QualityProfileItemsProps
  extends QualityProfileItemDragSourceActionProps {
  mode: EditQualityProfileMode;
  dragQualityIndex: string | null;
  dropQualityIndex: string | null;
  dropPosition: string | null;
  qualityProfileItems: Items;
  errors?: Failure[];
  warnings?: Failure[];
  onChangeMode: (mode: EditQualityProfileMode) => void;
}

function QualityProfileItems({
  mode,
  dropQualityIndex,
  dropPosition,
  qualityProfileItems,
  errors = [],
  warnings = [],
  onChangeMode,
  ...otherProps
}: QualityProfileItemsProps) {
  const [measureRef, { height: measuredHeight }] = useMeasure();
  const [defaultHeight, setDefaultHeight] = useState(0);
  const [editGroupsHeight, setEditGroupsHeight] = useState(0);
  const [editSizesHeight, setEditSizesHeight] = useState(0);

  const isDragging = dropQualityIndex !== null;
  const isDraggingUp = isDragging && dropPosition === 'above';
  const isDraggingDown = isDragging && dropPosition === 'below';

  const height = useMemo(() => {
    if (mode === 'default' && defaultHeight > 0) {
      return defaultHeight;
    } else if (mode === 'editGroups' && editGroupsHeight > 0) {
      return editGroupsHeight;
    } else if (mode === 'editSizes' && editSizesHeight > 0) {
      return editSizesHeight;
    }

    return 'auto';
  }, [mode, defaultHeight, editGroupsHeight, editSizesHeight]);

  const handleEditGroupsPress = useCallback(() => {
    onChangeMode('editGroups');
  }, [onChangeMode]);

  const handleEditSizesPress = useCallback(() => {
    onChangeMode('editSizes');
  }, [onChangeMode]);

  const handleDefaultModePress = useCallback(() => {
    onChangeMode('default');
  }, [onChangeMode]);

  useEffect(() => {
    if (mode === 'default') {
      setDefaultHeight(measuredHeight);
    } else if (mode === 'editGroups') {
      setEditGroupsHeight(measuredHeight);
    } else if (mode === 'editSizes') {
      setEditSizesHeight(measuredHeight);
    }
  }, [mode, measuredHeight]);

  return (
    <FormGroup size={sizes.EXTRA_SMALL}>
      <FormLabel size={sizes.SMALL}>{translate('Qualities')}</FormLabel>

      <div>
        <FormInputHelpText text={translate('QualitiesHelpText')} />

        {errors.map((error, index) => {
          return (
            <FormInputHelpText
              key={index}
              text={error.message}
              isError={true}
              isCheckInput={false}
            />
          );
        })}

        {warnings.map((warning, index) => {
          return (
            <FormInputHelpText
              key={index}
              text={warning.message}
              isWarning={true}
              isCheckInput={false}
            />
          );
        })}

        <Button
          className={styles.editGroupsButton}
          kind={kinds.PRIMARY}
          onPress={
            mode === 'editGroups'
              ? handleDefaultModePress
              : handleEditGroupsPress
          }
        >
          <div>
            <Icon
              className={styles.editButtonIcon}
              name={mode === 'editGroups' ? icons.REORDER : icons.GROUP}
            />

            {mode === 'editGroups'
              ? translate('DoneEditingGroups')
              : translate('EditGroups')}
          </div>
        </Button>

        <Button
          className={styles.editSizesButton}
          kind={kinds.PRIMARY}
          onPress={
            mode === 'editSizes' ? handleDefaultModePress : handleEditSizesPress
          }
        >
          <div>
            <Icon
              className={styles.editButtonIcon}
              name={mode === 'editSizes' ? icons.REORDER : icons.FILE}
            />

            {mode === 'editSizes'
              ? translate('DoneEditingSizes')
              : translate('EditSizes')}
          </div>
        </Button>

        <div
          ref={measureRef}
          className={styles.qualities}
          style={{ minHeight: `${height}px` }}
        >
          {qualityProfileItems
            .map((item, index) => {
              if ('quality' in item) {
                const { quality, allowed, minSize, maxSize, preferredSize } =
                  item;

                return (
                  <QualityProfileItemDragSource
                    key={item.quality.id}
                    {...otherProps}
                    mode={mode}
                    groupId={undefined}
                    qualityId={quality.id}
                    name={quality.name}
                    allowed={allowed}
                    minSize={minSize}
                    maxSize={maxSize}
                    preferredSize={preferredSize}
                    qualityIndex={`${index + 1}`}
                    isInGroup={false}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                  />
                );
              }

              const { id, name, allowed, items } = item;

              return (
                <QualityProfileItemDragSource
                  key={id}
                  {...otherProps}
                  mode={mode}
                  groupId={id}
                  qualityId={undefined}
                  name={name}
                  allowed={allowed}
                  items={items}
                  qualityIndex={`${index + 1}`}
                  isDraggingUp={isDraggingUp}
                  isDraggingDown={isDraggingDown}
                />
              );
            })
            .reverse()}
        </div>
      </div>
    </FormGroup>
  );
}

export default QualityProfileItems;
