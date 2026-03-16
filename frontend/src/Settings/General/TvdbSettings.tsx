import React, { useMemo } from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import { inputTypes, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface TvdbSettingsProps {
  tvdbApiKey: PendingSection<GeneralSettingsModel>['tvdbApiKey'];
  indexerOrderMatching: PendingSection<GeneralSettingsModel>['indexerOrderMatching'];
  onInputChange: (change: InputChanged) => void;
}

function TvdbSettings({
  tvdbApiKey,
  indexerOrderMatching,
  onInputChange,
}: TvdbSettingsProps) {
  const indexerOrderMatchingOptions = useMemo<
    EnhancedSelectInputValue<string>[]
  >(
    () => [
      {
        key: 'airedOrder',
        value: translate('AiredOrderDefault'),
      },
      {
        key: 'chosenOrder',
        value: translate('ChosenOrder'),
      },
    ],
    []
  );

  return (
    <FieldSet legend={translate('TvdbSettings')}>
      <FormGroup>
        <FormLabel>{translate('ApiKey')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="tvdbApiKey"
          helpText={translate('TvdbApiKeyHelpText')}
          onChange={onInputChange}
          {...tvdbApiKey}
        />
      </FormGroup>

      <FormGroup size={sizes.MEDIUM}>
        <FormLabel>{translate('IndexerEpisodeMatching')}</FormLabel>

        <FormInputGroup
          type={inputTypes.SELECT}
          name="indexerOrderMatching"
          values={indexerOrderMatchingOptions}
          helpText={translate('IndexerEpisodeMatchingHelpText')}
          onChange={onInputChange}
          {...indexerOrderMatching}
        />
      </FormGroup>
    </FieldSet>
  );
}

export default TvdbSettings;
