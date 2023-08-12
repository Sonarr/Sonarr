import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function ProxySettings(props) {
  const {
    settings,
    onInputChange
  } = props;

  const {
    proxyEnabled,
    proxyType,
    proxyHostname,
    proxyPort,
    proxyUsername,
    proxyPassword,
    proxyBypassFilter,
    proxyBypassLocalAddresses
  } = settings;

  const proxyTypeOptions = [
    {
      key: 'http',
      value: translate('HttpHttps')
    },
    {
      key: 'socks4',
      value: translate('Socks4')
    },
    {
      key: 'socks5',
      value: translate('Socks5')
    }
  ];

  return (
    <FieldSet legend={translate('Proxy')}>
      <FormGroup size={sizes.MEDIUM}>
        <FormLabel>{translate('UseProxy')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="proxyEnabled"
          onChange={onInputChange}
          {...proxyEnabled}
        />
      </FormGroup>

      {
        proxyEnabled.value &&
          <div>
            <FormGroup>
              <FormLabel>{translate('ProxyType')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="proxyType"
                values={proxyTypeOptions}
                onChange={onInputChange}
                {...proxyType}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Hostname')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="proxyHostname"

                onChange={onInputChange}
                {...proxyHostname}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Port')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="proxyPort"
                min={1}
                max={65535}
                onChange={onInputChange}
                {...proxyPort}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Username')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="proxyUsername"
                helpText={translate('ProxyUsernameHelpText')}
                onChange={onInputChange}
                {...proxyUsername}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Password')}</FormLabel>

              <FormInputGroup
                type={inputTypes.PASSWORD}
                name="proxyPassword"
                helpText={translate('ProxyPasswordHelpText')}
                onChange={onInputChange}
                {...proxyPassword}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IgnoredAddresses')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="proxyBypassFilter"
                helpText={translate('ProxyBypassFilterHelpText')}
                onChange={onInputChange}
                {...proxyBypassFilter}
              />
            </FormGroup>

            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('BypassProxyForLocalAddresses')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="proxyBypassLocalAddresses"
                onChange={onInputChange}
                {...proxyBypassLocalAddresses}
              />
            </FormGroup>
          </div>
      }
    </FieldSet>
  );
}

ProxySettings.propTypes = {
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default ProxySettings;
