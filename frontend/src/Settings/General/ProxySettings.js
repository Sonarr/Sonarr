import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

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
    { key: 'http', value: 'HTTP(S)' },
    { key: 'socks4', value: 'Socks4' },
    { key: 'socks5', value: 'Socks5 (Support TOR)' }
  ];

  return (
    <FieldSet legend="Proxy">
      <FormGroup size={sizes.MEDIUM}>
        <FormLabel>Use Proxy</FormLabel>

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
            <FormLabel>Proxy Type</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="proxyType"
              values={proxyTypeOptions}
              onChange={onInputChange}
              {...proxyType}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Hostname</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="proxyHostname"

              onChange={onInputChange}
              {...proxyHostname}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Port</FormLabel>

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
            <FormLabel>Username</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="proxyUsername"
              helpText="You only need to enter a username and password if one is required. Leave them blank otherwise."
              onChange={onInputChange}
              {...proxyUsername}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Password</FormLabel>

            <FormInputGroup
              type={inputTypes.PASSWORD}
              name="proxyPassword"
              helpText="You only need to enter a username and password if one is required. Leave them blank otherwise."
              onChange={onInputChange}
              {...proxyPassword}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Ignored Addresses</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="proxyBypassFilter"
              helpText="Use ',' as a separator, and '*.' as a wildcard for subdomains"
              onChange={onInputChange}
              {...proxyBypassFilter}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>Bypass Proxy for Local Addresses</FormLabel>

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
