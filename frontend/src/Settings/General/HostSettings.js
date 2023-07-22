import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

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
    <FieldSet legend={translate('Host')}>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('BindAddress')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="bindAddress"
          helpText={translate('BindAddressHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...bindAddress}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('PortNumber')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="port"
          min={1}
          max={65535}
          autocomplete="off"
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...port}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('UrlBase')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="urlBase"
          helpText={translate('UrlBaseHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...urlBase}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('InstanceName')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="instanceName"
          helpText={translate('InstanceNameHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...instanceName}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('ApplicationURL')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="applicationUrl"
          helpText={translate('ApplicationUrlHelpText')}
          onChange={onInputChange}
          {...applicationUrl}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
        size={sizes.MEDIUM}
      >
        <FormLabel>{translate('EnableSsl')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="enableSsl"
          helpText={translate('EnableSslHelpText')}
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
            <FormLabel>{translate('SslPort')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="sslPort"
              min={1}
              max={65535}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
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
            <FormLabel>{translate('SslCertPath')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="sslCertPath"
              helpText={translate('SslCertPathHelpText')}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
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
            <FormLabel>{translate('SslCertPassword')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PASSWORD}
              name="sslCertPassword"
              helpText={translate('SslCertPasswordHelpText')}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
              onChange={onInputChange}
              {...sslCertPassword}
            />
          </FormGroup> :
          null
      }

      {
        isWindows && mode !== 'service' ?
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('OpenBrowserOnStart')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="launchBrowser"
              helpText={translate('OpenBrowserOnStartHelpText')}
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
