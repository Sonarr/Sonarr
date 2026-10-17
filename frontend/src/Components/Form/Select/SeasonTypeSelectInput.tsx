import React, { useMemo } from 'react';
import { SeasonType } from 'Series/Series';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface SeasonTypeSelectInputProps {
  name: string;
  value: string;
  seasonTypes: SeasonType[];
  modalTitle?: string;
  isDisabled?: boolean;
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
  onChange: (payload: InputChanged<string>) => void;
}

function getSeasonTypeHint(seasonCount: number, episodeCount: number) {
  if (!episodeCount) {
    return seasonCount === 1
      ? translate('OneSeason')
      : translate('CountSeasons', { count: seasonCount });
  }

  if (seasonCount === 1) {
    return episodeCount === 1
      ? translate('OneSeasonOneEpisode')
      : translate('OneSeasonCountEpisodes', { episodeCount });
  }

  return episodeCount === 1
    ? translate('CountSeasonsOneEpisode', { seasonCount })
    : translate('CountSeasonsCountEpisodes', { seasonCount, episodeCount });
}

export default function SeasonTypeSelectInput(
  props: SeasonTypeSelectInputProps
) {
  const {
    value,
    seasonTypes,
    includeNoChange,
    includeNoChangeDisabled,
    includeMixed,
    onChange,
  } = props;

  const values = useMemo(() => {
    const result: EnhancedSelectInputValue<string>[] = seasonTypes.map(
      (seasonType) => {
        const seasonCount = seasonType.seasonNumbers.filter(
          (seasonNumber) => seasonNumber > 0
        ).length;
        const { episodeCount } = seasonType;

        return {
          key: seasonType.type,
          value: seasonType.name,
          hint: getSeasonTypeHint(seasonCount, episodeCount),
        };
      }
    );

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
  }, [seasonTypes, includeNoChange, includeNoChangeDisabled, includeMixed]);

  return (
    <EnhancedSelectInput
      {...props}
      value={value}
      values={values}
      onChange={onChange}
    />
  );
}
