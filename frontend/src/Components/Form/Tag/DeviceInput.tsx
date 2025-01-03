import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FormInputButton from 'Components/Form/FormInputButton';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import {
  clearOptions,
  defaultState,
  fetchOptions,
} from 'Store/Actions/providerOptionActions';
import { InputChanged } from 'typings/inputs';
import TagInput, { TagInputProps } from './TagInput';
import styles from './DeviceInput.css';

interface DeviceTag {
  id: string;
  name: string;
}

export interface DeviceInputProps extends TagInputProps<DeviceTag> {
  className?: string;
  name: string;
  value: string[];
  hasError?: boolean;
  hasWarning?: boolean;
  provider: string;
  providerData: object;
  onChange: (change: InputChanged<string[]>) => unknown;
}

function createDeviceTagsSelector(value: string[]) {
  return createSelector(
    (state: AppState) => state.providerOptions.devices || defaultState,
    (devices) => {
      return {
        ...devices,
        selectedDevices: value.map((valueDevice) => {
          const device = devices.items.find((d) => d.id === valueDevice);

          if (device) {
            return {
              id: device.id,
              name: `${device.name} (${device.id})`,
            };
          }

          return {
            id: valueDevice,
            name: `Unknown (${valueDevice})`,
          };
        }),
      };
    }
  );
}

function DeviceInput({
  className = styles.deviceInputWrapper,
  name,
  value,
  hasError,
  hasWarning,
  provider,
  providerData,
  onChange,
}: DeviceInputProps) {
  const dispatch = useDispatch();
  const { items, selectedDevices, isFetching } = useSelector(
    createDeviceTagsSelector(value)
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(
      fetchOptions({
        section: 'devices',
        action: 'getDevices',
        provider,
        providerData,
      })
    );
  }, [provider, providerData, dispatch]);

  const handleTagAdd = useCallback(
    (device: DeviceTag) => {
      // New tags won't have an ID, only a name.
      const deviceId = device.id || device.name;

      onChange({
        name,
        value: [...value, deviceId],
      });
    },
    [name, value, onChange]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      const newValue = value.slice();
      newValue.splice(index, 1);

      onChange({
        name,
        value: newValue,
      });
    },
    [name, value, onChange]
  );

  useEffect(() => {
    dispatch(
      fetchOptions({
        section: 'devices',
        action: 'getDevices',
        provider,
        providerData,
      })
    );

    return () => {
      dispatch(clearOptions({ section: 'devices' }));
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dispatch]);

  return (
    <div className={className}>
      <TagInput
        inputContainerClassName={styles.input}
        name={name}
        tags={selectedDevices}
        tagList={items}
        allowNew={true}
        minQueryLength={0}
        hasError={hasError}
        hasWarning={hasWarning}
        onTagAdd={handleTagAdd}
        onTagDelete={handleTagDelete}
      />

      <FormInputButton onPress={handleRefreshPress}>
        <Icon name={icons.REFRESH} isSpinning={isFetching} />
      </FormInputButton>
    </div>
  );
}

export default DeviceInput;
