import React, { useMemo } from 'react';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';
import SeriesTypeSelectInputOption from './SeriesTypeSelectInputOption';
import SeriesTypeSelectInputSelectedValue from './SeriesTypeSelectInputSelectedValue';

interface SeriesTypeSelectInputProps
  extends EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string> {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
}

export interface ISeriesTypeOption {
  key: string;
  value: string;
  format?: string;
  isDisabled?: boolean;
}

const seriesTypeOptions: ISeriesTypeOption[] = [
  {
    key: seriesTypes.STANDARD,
    value: 'Standard',
    get format() {
      return translate('StandardEpisodeTypeFormat', { format: 'S01E05' });
    },
  },
  {
    key: seriesTypes.DAILY,
    value: 'Daily / Date',
    get format() {
      return translate('DailyEpisodeTypeFormat', { format: '2020-05-25' });
    },
  },
  {
    key: seriesTypes.ANIME,
    value: 'Anime / Absolute',
    get format() {
      return translate('AnimeEpisodeTypeFormat', { format: '005' });
    },
  },
];

function SeriesTypeSelectInput(props: SeriesTypeSelectInputProps) {
  const {
    includeNoChange = false,
    includeNoChangeDisabled = true,
    includeMixed = false,
  } = props;

  const values = useMemo(() => {
    const result = [...seriesTypeOptions];

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      result.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return result;
  }, [includeNoChange, includeNoChangeDisabled, includeMixed]);

  return (
    <EnhancedSelectInput
      {...props}
      values={values}
      optionComponent={SeriesTypeSelectInputOption}
      selectedValueComponent={SeriesTypeSelectInputSelectedValue}
    />
  );
}

export default SeriesTypeSelectInput;
