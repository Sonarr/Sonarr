import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';

function HostSettings(props) {
  const {
    advancedSettings,
    settings,
    isWindows,
    mode,
    onInputChange
  } = props;

  const {
    bindAddress,
    port,
    urlBase,
    instanceName,
    applicationUrl,
    enableSsl,
    sslPort,
    sslCertPath,
    sslCertPassword,
    launchBrowser
  } = settings;

  return (
    <FieldSet legend="Host">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Bind Address</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="bindAddress"
          helpText="Valid IPv4 address or '*' for all interfaces"
          helpTextWarning="Requires restart to take effect"
          onChange={onInputChange}
          {...bindAddress}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>Port Number</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="port"
          min={1}
          max={65535}
          autocomplete="off"
          helpTextWarning="Requires restart to take effect"
          onChange={onInputChange}
          {...port}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>URL Base</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="urlBase"
          helpText="For reverse proxy support, default is empty"
          helpTextWarning="Requires restart to take effect"
          onChange={onInputChange}
          {...urlBase}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Instance Name</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="instanceName"
          helpText="Instance name in tab and for Syslog app name"
          helpTextWarning="Requires restart to take effect"
          onChange={onInputChange}
          {...instanceName}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Application URL</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="applicationUrl"
          helpText="This application's external URL including http(s)://, port and URL base"
          onChange={onInputChange}
          {...applicationUrl}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
        size={sizes.MEDIUM}
      >
        <FormLabel>Enable SSL</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="enableSsl"
          helpText=" Requires restart running as administrator to take effect"
          onChange={onInputChange}
          {...enableSsl}
        />
      </FormGroup>

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>SSL Port</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="sslPort"
              min={1}
              max={65535}
              helpTextWarning="Requires restart to take effect"
              onChange={onInputChange}
              {...sslPort}
            />
          </FormGroup> :
          null
      }

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>SSL Cert Path</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="sslCertPath"
              helpText="Path to pfx file"
              helpTextWarning="Requires restart to take effect"
              onChange={onInputChange}
              {...sslCertPath}
            />
          </FormGroup> :
          null
      }

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>SSL Cert Password</FormLabel>

            <FormInputGroup
              type={inputTypes.PASSWORD}
              name="sslCertPassword"
              helpText="Password for pfx file"
              helpTextWarning="Requires restart to take effect"
              onChange={onInputChange}
              {...sslCertPassword}
            />
          </FormGroup> :
          null
      }

      {
        isWindows && mode !== 'service' ?
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>Open browser on start</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="launchBrowser"
              helpText=" Open a web browser and navigate to Sonarr homepage on app start."
              onChange={onInputChange}
              {...launchBrowser}
            />
          </FormGroup> :
          null
      }

    </FieldSet>
  );
}

HostSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  isWindows: PropTypes.bool.isRequired,
  mode: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default HostSettings;
