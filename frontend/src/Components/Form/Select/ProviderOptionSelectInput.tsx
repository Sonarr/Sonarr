import React, { useMemo } from 'react';
import useProviderOptions, {
  ProviderOptions,
} from 'Settings/useProviderOptions';
import { FieldSelectOption } from 'typings/Field';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

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

export type ProviderActionType =
  | 'devices'
  | 'servers'
  | 'newznabCategories'
  | 'getProfiles'
  | 'getTags'
  | 'getRootFolders';

export interface ProviderOptionSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<unknown>, unknown>,
    'values'
  > {
  provider: string;
  providerData: ProviderOptions;
  name: string;
  value: unknown;
  selectOptionsProviderAction: ProviderActionType;
}

function ProviderOptionSelectInput({
  provider,
  providerData,
  selectOptionsProviderAction,
  ...otherProps
}: ProviderOptionSelectInputProps) {
  const { data, isFetching } = useProviderOptions({
    provider,
    action: selectOptionsProviderAction,
    providerData,
  });

  const values = useMemo(() => getSelectOptions(data), [data]);

  return (
    <EnhancedSelectInput
      {...otherProps}
      isFetching={isFetching}
      values={values}
    />
  );
}

export default ProviderOptionSelectInput;
