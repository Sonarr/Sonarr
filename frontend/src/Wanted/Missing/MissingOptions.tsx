import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import { gotoMissingPage, setMissingOption } from 'Store/Actions/wantedActions';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

function MissingOptions() {
  const dispatch = useDispatch();
  const { includeSpecials } = useSelector(
    (state: AppState) => state.wanted.missing.options
  );

  const handleOptionChange = useCallback(
    ({ name, value }: InputChanged<boolean>) => {
      dispatch(
        setMissingOption({
          [name]: value,
        })
      );

      if (name === 'includeSpecials') {
        dispatch(gotoMissingPage({ page: 1 }));
      }
    },
    [dispatch]
  );

  return (
    <FormGroup>
      <FormLabel>{translate('ShowSpecials')}</FormLabel>

      <FormInputGroup
        type={inputTypes.CHECK}
        name="includeSpecials"
        value={includeSpecials}
        helpText={translate('ShowSpecialMissingEpisodesHelpText')}
        onChange={handleOptionChange}
      />
    </FormGroup>
  );
}

export default MissingOptions;
