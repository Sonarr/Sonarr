import React, { Fragment, useCallback } from 'react';
import { useSelector } from 'react-redux';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import selectTableOptions from './selectTableOptions';

interface SeriesIndexTableOptionsProps {
  onTableOptionChange(...args: unknown[]): unknown;
}

function SeriesIndexTableOptions(props: SeriesIndexTableOptionsProps) {
  const { onTableOptionChange } = props;

  const tableOptions = useSelector(selectTableOptions);

  const { showBanners, showSearchAction } = tableOptions;

  const onTableOptionChangeWrapper = useCallback(
    ({ name, value }) => {
      onTableOptionChange({
        tableOptions: {
          ...tableOptions,
          [name]: value,
        },
      });
    },
    [tableOptions, onTableOptionChange]
  );

  return (
    <Fragment>
      <FormGroup>
        <FormLabel>Show Banners</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="showBanners"
          value={showBanners}
          helpText="Show banners instead of titles"
          onChange={onTableOptionChangeWrapper}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>Show Search</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="showSearchAction"
          value={showSearchAction}
          helpText="Show search button on hover"
          onChange={onTableOptionChangeWrapper}
        />
      </FormGroup>
    </Fragment>
  );
}

export default SeriesIndexTableOptions;
