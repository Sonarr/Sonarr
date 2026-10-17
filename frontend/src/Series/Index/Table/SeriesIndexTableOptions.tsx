import React, { useCallback } from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes } from 'Helpers/Props';
import {
  setSeriesTableOptions,
  useSeriesTableOptions,
} from 'Series/seriesOptionsStore';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

function SeriesIndexTableOptions() {
  const { showBanners, showSearchAction } = useSeriesTableOptions();

  const handleTableOptionChange = useCallback(
    ({ name, value }: InputChanged<boolean>) => {
      setSeriesTableOptions({
        [name]: value,
      });
    },
    []
  );

  return (
    <>
      <FormRow>
        <FormLabel>{translate('ShowBanners')}</FormLabel>
        <FormInputHelpText text={translate('ShowBannersHelpText')} />
        <FormInput
          type={inputTypes.CHECK}
          name="showBanners"
          value={showBanners}
          onChange={handleTableOptionChange}
        />
      </FormRow>
      <FormRow>
        <FormLabel>{translate('ShowSearch')}</FormLabel>
        <FormInputHelpText text={translate('ShowSearchHelpText')} />
        <FormInput
          type={inputTypes.CHECK}
          name="showSearchAction"
          value={showSearchAction}
          onChange={handleTableOptionChange}
        />
      </FormRow>
    </>
  );
}

export default SeriesIndexTableOptions;
