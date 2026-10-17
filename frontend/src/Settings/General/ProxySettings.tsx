import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import { inputTypes, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface ProxySettingsProps {
  proxyEnabled: PendingSection<GeneralSettingsModel>['proxyEnabled'];
  proxyType: PendingSection<GeneralSettingsModel>['proxyType'];
  proxyHostname: PendingSection<GeneralSettingsModel>['proxyHostname'];
  proxyPort: PendingSection<GeneralSettingsModel>['proxyPort'];
  proxyUsername: PendingSection<GeneralSettingsModel>['proxyUsername'];
  proxyPassword: PendingSection<GeneralSettingsModel>['proxyPassword'];
  proxyBypassFilter: PendingSection<GeneralSettingsModel>['proxyBypassFilter'];
  proxyBypassLocalAddresses: PendingSection<GeneralSettingsModel>['proxyBypassLocalAddresses'];
  onInputChange: (change: InputChanged) => void;
}

function ProxySettings({
  proxyEnabled,
  proxyType,
  proxyHostname,
  proxyPort,
  proxyUsername,
  proxyPassword,
  proxyBypassFilter,
  proxyBypassLocalAddresses,
  onInputChange,
}: ProxySettingsProps) {
  const proxyTypeOptions: EnhancedSelectInputValue<string>[] = [
    {
      key: 'http',
      value: translate('HttpHttps'),
    },
    {
      key: 'socks4',
      value: translate('Socks4'),
    },
    {
      key: 'socks5',
      value: translate('Socks5'),
    },
  ];

  return (
    <FieldSet legend={translate('Proxy')} caption={translate('ProxyCaption')}>
      <FormRow size={sizes.MEDIUM}>
        <FormLabel>{translate('UseProxy')}</FormLabel>

        <FormInput
          type={inputTypes.CHECK}
          name="proxyEnabled"
          onChange={onInputChange}
          {...proxyEnabled}
        />
      </FormRow>
      {proxyEnabled.value && (
        <div>
          <FormRow>
            <FormLabel>{translate('ProxyType')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="proxyType"
              values={proxyTypeOptions}
              onChange={onInputChange}
              {...proxyType}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Hostname')}</FormLabel>

            <FormInput
              type={inputTypes.TEXT}
              name="proxyHostname"
              onChange={onInputChange}
              {...proxyHostname}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Port')}</FormLabel>

            <FormInput
              type={inputTypes.NUMBER}
              name="proxyPort"
              min={1}
              max={65535}
              onChange={onInputChange}
              {...proxyPort}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Username')}</FormLabel>
            <FormInputHelpText text={translate('ProxyUsernameHelpText')} />
            <FormInput
              type={inputTypes.TEXT}
              name="proxyUsername"
              onChange={onInputChange}
              {...proxyUsername}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Password')}</FormLabel>
            <FormInputHelpText text={translate('ProxyPasswordHelpText')} />
            <FormInput
              type={inputTypes.PASSWORD}
              name="proxyPassword"
              onChange={onInputChange}
              {...proxyPassword}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('IgnoredAddresses')}</FormLabel>
            <FormInputHelpText text={translate('ProxyBypassFilterHelpText')} />
            <FormInput
              type={inputTypes.TEXT}
              name="proxyBypassFilter"
              onChange={onInputChange}
              {...proxyBypassFilter}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('BypassProxyForLocalAddresses')}</FormLabel>

            <FormInput
              type={inputTypes.CHECK}
              name="proxyBypassLocalAddresses"
              onChange={onInputChange}
              {...proxyBypassLocalAddresses}
            />
          </FormRow>
        </div>
      )}
    </FieldSet>
  );
}

export default ProxySettings;
