import React, { HTMLProps, useCallback, useRef, useState } from 'react';
import ReactSlider from 'react-slider';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import NumberInput from 'Components/Form/NumberInput';
import Popover from 'Components/Tooltip/Popover';
import { tooltipPositions } from 'Helpers/Props';
import QualityDefinitionLimits from 'Settings/Quality/Definition/QualityDefinitionLimits';
import { InputChanged } from 'typings/inputs';
import { Failure } from 'typings/pending';
import formatBytes from 'Utilities/Number/formatBytes';
import roundNumber from 'Utilities/Number/roundNumber';
import translate from 'Utilities/String/translate';
import {
  emptyItemFailures,
  ItemFailures,
  SizeField,
} from './qualityProfileItemFailures';
import styles from './QualityProfileItemSize.module.css';

const MIN = 0;
const MAX = 1000;
const STEP_SIZE = 0.1;
const MIN_DISTANCE = 3;
const SIZE_GAP = 5;
const SLIDER_MAX = roundNumber(Math.pow(MAX, 1 / 1.1));
const SLIDER_PREFERRED_DEFAULT = SLIDER_MAX - MIN_DISTANCE;

interface SizeProps {
  minSize: number | null;
  preferredSize: number | null;
  maxSize: number | null;
}

export interface SizeChanged extends SizeProps {
  qualityId: number;
}

interface PendingSizeChange {
  field: SizeField;
  sizes: SizeProps;
}

export interface QualityProfileItemSizeProps {
  id: number;
  minSize: number | null;
  preferredSize: number | null;
  maxSize: number | null;
  failures?: ItemFailures;
  onSizeChange: (props: SizeChanged) => void;
}

function trackRenderer({ key, ...props }: HTMLProps<HTMLDivElement>) {
  return <div key={key} {...props} className={styles.track} />;
}

function thumbRenderer({ key, ...props }: HTMLProps<HTMLDivElement>) {
  return <div key={key} {...props} className={styles.thumb} />;
}

function getSliderValue(value: number | null, defaultValue: number): number {
  const sliderValue =
    value != null && value > 0 ? Math.pow(value, 1 / 1.1) : defaultValue;

  return roundNumber(sliderValue);
}

function getSliderSizes({
  minSize,
  preferredSize,
  maxSize,
}: SizeProps): SizeProps {
  return {
    minSize: getSliderValue(minSize, MIN),
    preferredSize: getSliderValue(preferredSize, SLIDER_PREFERRED_DEFAULT),
    maxSize: getSliderValue(maxSize, SLIDER_MAX),
  };
}

function adjustSizes(field: SizeField, sizes: SizeProps): SizeProps {
  const { minSize, preferredSize, maxSize } = sizes;

  if (field === 'minSize') {
    const floor = roundNumber((minSize ?? MIN) + SIZE_GAP);
    const newPreferredSize =
      preferredSize == null ? null : Math.max(preferredSize, floor);

    return {
      minSize,
      preferredSize: newPreferredSize,
      maxSize:
        maxSize == null
          ? null
          : Math.max(
              maxSize,
              roundNumber((newPreferredSize ?? floor) + SIZE_GAP)
            ),
    };
  }

  if (field === 'maxSize') {
    if (maxSize == null) {
      return sizes;
    }

    const ceiling = roundNumber(maxSize - SIZE_GAP);
    const newPreferredSize =
      preferredSize == null ? null : Math.min(preferredSize, ceiling);

    return {
      minSize:
        minSize == null
          ? null
          : Math.min(
              minSize,
              roundNumber((newPreferredSize ?? ceiling) - SIZE_GAP)
            ),
      preferredSize: newPreferredSize,
      maxSize,
    };
  }

  if (preferredSize == null) {
    return sizes;
  }

  return {
    minSize:
      minSize == null
        ? null
        : Math.min(minSize, roundNumber(preferredSize - SIZE_GAP)),
    preferredSize,
    maxSize:
      maxSize == null
        ? null
        : Math.max(maxSize, roundNumber(preferredSize + SIZE_GAP)),
  };
}

function renderFailures(failures: Failure[], isError: boolean) {
  return failures.map((failure, index) => {
    return (
      <FormInputHelpText
        key={`${isError ? 'error' : 'warning'}-${index}`}
        text={failure.message}
        isError={isError}
        isWarning={!isError}
        isCheckInput={false}
      />
    );
  });
}

export default function QualityProfileItemSize({
  id,
  minSize,
  maxSize,
  preferredSize,
  failures = emptyItemFailures(),
  onSizeChange,
}: QualityProfileItemSizeProps) {
  const [sizes, setSizes] = useState<SizeProps>(
    getSliderSizes({ minSize, preferredSize, maxSize })
  );

  const handleSliderChange = useCallback(
    ([sliderMinSize, sliderPreferredSize, sliderMaxSize]: [
      number,
      number,
      number
    ]) => {
      setSizes({
        minSize: sliderMinSize,
        preferredSize: sliderPreferredSize,
        maxSize: sliderMaxSize,
      });

      onSizeChange?.({
        qualityId: id,
        minSize: roundNumber(Math.pow(sliderMinSize, 1.1)),
        preferredSize:
          sliderPreferredSize === SLIDER_PREFERRED_DEFAULT
            ? null
            : roundNumber(Math.pow(sliderPreferredSize, 1.1)),
        maxSize:
          sliderMaxSize === SLIDER_MAX
            ? null
            : roundNumber(Math.pow(sliderMaxSize, 1.1)),
      });
    },
    [id, setSizes, onSizeChange]
  );

  const pendingChange = useRef<PendingSizeChange | null>(null);

  const handleInputChange = useCallback(
    (field: SizeField, value: number | null) => {
      const newSizes = { minSize, preferredSize, maxSize, [field]: value };

      pendingChange.current = { field, sizes: newSizes };
      setSizes(getSliderSizes(newSizes));

      onSizeChange?.({
        qualityId: id,
        ...newSizes,
      });
    },
    [id, minSize, preferredSize, maxSize, setSizes, onSizeChange]
  );

  const handleInputBlur = useCallback(() => {
    if (!pendingChange.current) {
      return;
    }

    const { field, sizes: newSizes } = pendingChange.current;
    const adjustedSizes = adjustSizes(field, newSizes);

    pendingChange.current = null;
    setSizes(getSliderSizes(adjustedSizes));

    onSizeChange?.({
      qualityId: id,
      ...adjustedSizes,
    });
  }, [id, setSizes, onSizeChange]);

  const handleMinSizeChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      handleInputChange('minSize', value);
    },
    [handleInputChange]
  );

  const handlePreferredSizeChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      handleInputChange('preferredSize', value);
    },
    [handleInputChange]
  );

  const handleMaxSizeChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      handleInputChange('maxSize', value);
    },
    [handleInputChange]
  );

  const handleAfterSliderChange = useCallback(() => {
    setSizes(getSliderSizes({ minSize, preferredSize, maxSize }));
  }, [minSize, maxSize, preferredSize, setSizes]);

  const minBytes = (minSize ?? 0) * 1024 * 1024;
  const minSixty = `${formatBytes(minBytes * 60)}/${translate(
    'HourShorthand'
  )}`;

  const preferredBytes = (preferredSize ?? 0) * 1024 * 1024;
  const preferredSixty = preferredBytes
    ? `${formatBytes(preferredBytes * 60)}/${translate('HourShorthand')}`
    : translate('Unlimited');

  const maxBytes = maxSize == null ? null : maxSize * 1024 * 1024;
  const maxSixty = maxBytes
    ? `${formatBytes(maxBytes * 60)}/${translate('HourShorthand')}`
    : translate('Unlimited');

  const allFailures = [
    ...failures.errors.minSize,
    ...failures.errors.preferredSize,
    ...failures.errors.maxSize,
    ...failures.warnings.minSize,
    ...failures.warnings.preferredSize,
    ...failures.warnings.maxSize,
  ];

  return (
    <div className={styles.sizeLimit}>
      {/* eslint-disable-next-line @typescript-eslint/ban-ts-comment */}
      {/* @ts-ignore React version mismatch */}
      <ReactSlider
        className={styles.slider}
        min={MIN}
        max={SLIDER_MAX}
        step={STEP_SIZE}
        minDistance={3}
        value={[sizes.minSize, sizes.preferredSize, sizes.maxSize]}
        withTracks={true}
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore allowCross is still available in the version currently used
        allowCross={false}
        snapDragDisabled={true}
        pearling={true}
        renderThumb={thumbRenderer}
        renderTrack={trackRenderer}
        onChange={handleSliderChange}
        onAfterChange={handleAfterSliderChange}
      />

      <div className={styles.grid}>
        <div className={styles.column}>
          <span className={styles.caption}>{translate('Minimum')}</span>

          <div className={styles.field}>
            <NumberInput
              className={styles.sizeInput}
              name={`${id}.min`}
              value={minSize ?? MIN}
              min={MIN}
              max={MAX - SIZE_GAP * 2}
              step={0.1}
              isFloat={true}
              onChange={handleMinSizeChange}
              onBlur={handleInputBlur}
            />

            <span className={styles.unit}>
              MiB/{translate('MinuteShorthand')}
            </span>
          </div>

          <Popover
            anchor={<span className={styles.human}>{minSixty}</span>}
            title={translate('MinimumLimits')}
            body={
              <QualityDefinitionLimits
                bytes={minBytes}
                message={translate('NoMinimumForAnyRuntime')}
              />
            }
            position={tooltipPositions.BOTTOM}
          />
        </div>

        <div className={styles.column}>
          <span className={styles.caption}>{translate('Preferred')}</span>

          <div className={styles.field}>
            <NumberInput
              className={styles.sizeInput}
              name={`${id}.preferred`}
              value={preferredSize ?? MAX - 5}
              min={MIN + SIZE_GAP}
              max={MAX - SIZE_GAP}
              step={0.1}
              isFloat={true}
              onChange={handlePreferredSizeChange}
              onBlur={handleInputBlur}
            />

            <span className={styles.unit}>
              MiB/{translate('MinuteShorthand')}
            </span>
          </div>

          <Popover
            anchor={<span className={styles.human}>{preferredSixty}</span>}
            title={translate('PreferredSize')}
            body={
              <QualityDefinitionLimits
                bytes={preferredBytes}
                message={translate('NoLimitForAnyRuntime')}
              />
            }
            position={tooltipPositions.BOTTOM}
          />
        </div>

        <div className={styles.column}>
          <span className={styles.caption}>{translate('Maximum')}</span>

          <div className={styles.field}>
            <NumberInput
              className={styles.sizeInput}
              name={`${id}.max`}
              value={maxSize ?? MAX}
              min={MIN + SIZE_GAP * 2}
              max={MAX}
              step={0.1}
              isFloat={true}
              onChange={handleMaxSizeChange}
              onBlur={handleInputBlur}
            />

            <span className={styles.unit}>
              MiB/{translate('MinuteShorthand')}
            </span>
          </div>

          <Popover
            anchor={<span className={styles.human}>{maxSixty}</span>}
            title={translate('MaximumLimits')}
            body={
              <QualityDefinitionLimits
                bytes={maxBytes ?? 0}
                message={translate('NoLimitForAnyRuntime')}
              />
            }
            position={tooltipPositions.BOTTOM}
          />
        </div>
      </div>

      {allFailures.length > 0 ? (
        <div className={styles.failures}>
          {renderFailures(failures.errors.minSize, true)}
          {renderFailures(failures.errors.preferredSize, true)}
          {renderFailures(failures.errors.maxSize, true)}
          {renderFailures(failures.warnings.minSize, false)}
          {renderFailures(failures.warnings.preferredSize, false)}
          {renderFailures(failures.warnings.maxSize, false)}
        </div>
      ) : null}
    </div>
  );
}
