import { isEqual } from 'lodash';
import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import ProviderOptionsAppState, {
  ProviderOptions,
} from 'App/State/ProviderOptionsAppState';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  clearOptions,
  fetchOptions,
} from 'Store/Actions/providerOptionActions';
import { FieldSelectOption } from 'typings/Field';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

const importantFieldNames = ['baseUrl', 'apiPath', 'apiKey', 'authToken'];

function getProviderDataKey(providerData: ProviderOptions) {
  if (!providerData || !providerData.fields) {
    return null;
  }

  const fields = providerData.fields
    .filter((f) => importantFieldNames.includes(f.name))
    .map((f) => f.value);

  return fields;
}

function getSelectOptions(items: FieldSelectOption<unknown>[]) {
  if (!items) {
    return [];
  }

  return items.map((option) => {
    return {
      key: option.value,
      value: option.name,
      hint: option.hint,
      parentKey: option.parentValue,
      isDisabled: option.isDisabled,
      additionalProperties: option.additionalProperties,
    };
  });
}

function createProviderOptionsSelector(
  selectOptionsProviderAction: keyof Omit<ProviderOptionsAppState, 'devices'>
) {
  return createSelector(
    (state: AppState) => state.providerOptions[selectOptionsProviderAction],
    (options) => {
      if (!options) {
        return {
          isFetching: false,
          values: [],
        };
      }

      return {
        isFetching: options.isFetching,
        values: getSelectOptions(options.items),
      };
    }
  );
}

export interface ProviderOptionSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<unknown>, unknown>,
    'values'
  > {
  provider: string;
  providerData: ProviderOptions;
  name: string;
  value: unknown;
  selectOptionsProviderAction: keyof Omit<ProviderOptionsAppState, 'devices'>;
}

function ProviderOptionSelectInput({
  provider,
  providerData,
  selectOptionsProviderAction,
  ...otherProps
}: ProviderOptionSelectInputProps) {
  const dispatch = useDispatch();
  const [isRefetchRequired, setIsRefetchRequired] = useState(false);
  const previousProviderData = usePrevious(providerData);
  const { isFetching, values } = useSelector(
    createProviderOptionsSelector(selectOptionsProviderAction)
  );

  const handleOpen = useCallback(() => {
    if (isRefetchRequired && selectOptionsProviderAction) {
      setIsRefetchRequired(false);

      dispatch(
        fetchOptions({
          section: selectOptionsProviderAction,
          action: selectOptionsProviderAction,
          provider,
          providerData,
        })
      );
    }
  }, [
    isRefetchRequired,
    provider,
    providerData,
    selectOptionsProviderAction,
    dispatch,
  ]);

  useEffect(() => {
    if (selectOptionsProviderAction) {
      dispatch(
        fetchOptions({
          section: selectOptionsProviderAction,
          action: selectOptionsProviderAction,
          provider,
          providerData,
        })
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectOptionsProviderAction, dispatch]);

  useEffect(() => {
    if (!previousProviderData) {
      return;
    }

    const prevKey = getProviderDataKey(previousProviderData);
    const nextKey = getProviderDataKey(providerData);

    if (!isEqual(prevKey, nextKey)) {
      setIsRefetchRequired(true);
    }
  }, [providerData, previousProviderData, setIsRefetchRequired]);

  useEffect(() => {
    return () => {
      if (selectOptionsProviderAction) {
        dispatch(clearOptions({ section: selectOptionsProviderAction }));
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <EnhancedSelectInput
      {...otherProps}
      isFetching={isFetching}
      values={values}
      onOpen={handleOpen}
    />
  );
}

export default ProviderOptionSelectInput;
