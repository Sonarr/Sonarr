import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import { gotoQueuePage, setQueueOption } from 'Store/Actions/queueActions';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

function QueueOptions() {
  const dispatch = useDispatch();
  const { includeUnknownSeriesItems } = useSelector(
    (state: AppState) => state.queue.options
  );

  const handleOptionChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      dispatch(
        setQueueOption({
          [name]: value,
        })
      );

      if (name === 'includeUnknownSeriesItems') {
        dispatch(gotoQueuePage({ page: 1 }));
      }
    },
    [dispatch]
  );

  return (
    <FormGroup>
      <FormLabel>{translate('ShowUnknownSeriesItems')}</FormLabel>

      <FormInputGroup
        type={inputTypes.CHECK}
        name="includeUnknownSeriesItems"
        value={includeUnknownSeriesItems}
        helpText={translate('ShowUnknownSeriesItemsHelpText')}
        onChange={handleOptionChange}
      />
    </FormGroup>
  );
}

export default QueueOptions;
