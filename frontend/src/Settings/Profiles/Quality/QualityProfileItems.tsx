import React, { MouseEvent, useCallback, useMemo } from 'react';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import { Failure } from 'typings/pending';
import translate from 'Utilities/String/translate';
import QualityProfileItemDragSource, {
  QualityProfileItemDragSourceActionProps,
} from './QualityProfileItemDragSource';
import { getItemFailures, ItemFailuresMap } from './qualityProfileItemFailures';
import { QualityProfileItems as Items } from './useQualityProfiles';
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
  itemFailures: ItemFailuresMap;
  onChangeMode: (mode: EditQualityProfileMode) => void;
}

interface ModeOption {
  key: EditQualityProfileMode;
  label: string;
}

function QualityProfileItems({
  mode,
  dropQualityIndex,
  dropPosition,
  qualityProfileItems,
  errors = [],
  warnings = [],
  itemFailures,
  onChangeMode,
  ...otherProps
}: QualityProfileItemsProps) {
  const isDragging = dropQualityIndex !== null;
  const isDraggingUp = isDragging && dropPosition === 'above';
  const isDraggingDown = isDragging && dropPosition === 'below';

  const modeOptions: ModeOption[] = useMemo(
    () => [
      { key: 'default', label: translate('Qualities') },
      { key: 'editGroups', label: translate('Groups') },
      { key: 'editSizes', label: translate('Sizes') },
    ],
    []
  );

  const handleSelectMode = useCallback(
    (event: MouseEvent<HTMLButtonElement>) => {
      const next = event.currentTarget.dataset
        .modeKey as EditQualityProfileMode;

      if (next) {
        onChangeMode(next);
      }
    },
    [onChangeMode]
  );

  return (
    <div className={styles.qualitiesSection} data-size={sizes.EXTRA_SMALL}>
      <div className={styles.headingRow}>
        <div className={styles.headingCluster}>
          <h3 className={styles.heading}>{translate('Qualities')}</h3>

          {itemFailures.size > 0 ? (
            <Label
              kind={kinds.DANGER}
              size={sizes.MEDIUM}
              title={translate('QualityProfileItemFailureCount', {
                count: itemFailures.size,
              })}
            >
              {itemFailures.size}
            </Label>
          ) : null}
        </div>

        <div className={styles.modeCluster} role="group">
          {modeOptions.map((option) => {
            const isActive = mode === option.key;
            return (
              <button
                key={option.key}
                type="button"
                className={isActive ? styles.modeCellActive : styles.modeCell}
                data-mode-key={option.key}
                aria-pressed={isActive}
                onClick={handleSelectMode}
              >
                {option.label}
              </button>
            );
          })}
        </div>
      </div>

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

      <div className={styles.qualities}>
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
                  failures={getItemFailures(itemFailures, index)}
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
                itemFailures={itemFailures}
                qualityIndex={`${index + 1}`}
                isDraggingUp={isDraggingUp}
                isDraggingDown={isDraggingDown}
              />
            );
          })
          .reverse()}
      </div>
    </div>
  );
}

export default QualityProfileItems;
