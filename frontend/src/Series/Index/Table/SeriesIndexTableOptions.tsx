import React, { useCallback } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
      <FormGroup>
        <FormLabel>{translate('ShowBanners')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="showBanners"
          value={showBanners}
          helpText={translate('ShowBannersHelpText')}
          onChange={handleTableOptionChange}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('ShowSearch')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="showSearchAction"
          value={showSearchAction}
          helpText={translate('ShowSearchHelpText')}
          onChange={handleTableOptionChange}
        />
      </FormGroup>
    </>
  );
}

export default SeriesIndexTableOptions;
