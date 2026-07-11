import React, { MouseEvent, useCallback, useMemo } from 'react';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import { Failure } from 'typings/pending';
import translate from 'Utilities/String/translate';
import QualityProfileItem from './QualityProfileItem';
import { ItemFailures } from './qualityProfileItemFailures';
import QualityProfileItemGroup from './QualityProfileItemGroup';
import { SizeChanged } from './QualityProfileItemSize';
import { DisplayItem, ROOT_CONTAINER } from './useQualityProfileDnd';
import styles from './QualityProfileItems.css';

export type EditQualityProfileMode = 'default' | 'editGroups' | 'editSizes';

interface QualityProfileItemsProps {
  mode: EditQualityProfileMode;
  displayItems: DisplayItem[];
  errors?: Failure[];
  warnings?: Failure[];
  failuresByQualityId: Map<number, ItemFailures>;
  onChangeMode: (mode: EditQualityProfileMode) => void;
  onCreateGroupPress: (qualityId: number) => void;
  onItemAllowedChange: (id: number, allowed: boolean) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onGroupAllowedChange: (id: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
}

interface ModeOption {
  key: EditQualityProfileMode;
  label: string;
}

function QualityProfileItems({
  mode,
  displayItems,
  errors = [],
  warnings = [],
  failuresByQualityId,
  onChangeMode,
  onCreateGroupPress,
  onItemAllowedChange,
  onDeleteGroupPress,
  onGroupAllowedChange,
  onItemGroupNameChange,
  onSizeChange,
}: QualityProfileItemsProps) {
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

          {failuresByQualityId.size > 0 ? (
            <Label
              kind={kinds.DANGER}
              size={sizes.MEDIUM}
              title={translate('QualityProfileItemFailureCount', {
                count: failuresByQualityId.size,
              })}
            >
              {failuresByQualityId.size}
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
        {displayItems.map((entry, index) => {
          if (entry.kind === 'group') {
            return (
              <QualityProfileItemGroup
                key={entry.group.id}
                mode={mode}
                index={index}
                groupId={entry.group.id}
                name={entry.group.name}
                allowed={entry.group.allowed}
                items={entry.items}
                failuresByQualityId={failuresByQualityId}
                onGroupAllowedChange={onGroupAllowedChange}
                onItemAllowedChange={onItemAllowedChange}
                onItemGroupNameChange={onItemGroupNameChange}
                onDeleteGroupPress={onDeleteGroupPress}
                onSizeChange={onSizeChange}
              />
            );
          }

          const { quality, allowed, minSize, maxSize, preferredSize } =
            entry.item;

          return (
            <QualityProfileItem
              key={quality.id}
              mode={mode}
              index={index}
              containerId={ROOT_CONTAINER}
              qualityId={quality.id}
              name={quality.name}
              allowed={allowed}
              minSize={minSize}
              maxSize={maxSize}
              preferredSize={preferredSize}
              failures={failuresByQualityId.get(quality.id)}
              onCreateGroupPress={onCreateGroupPress}
              onItemAllowedChange={onItemAllowedChange}
              onSizeChange={onSizeChange}
            />
          );
        })}
      </div>
    </div>
  );
}

export default QualityProfileItems;
