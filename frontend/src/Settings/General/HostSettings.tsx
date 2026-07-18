import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes, sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { useIsWindowsService } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface HostSettingsProps {
  bindAddress: PendingSection<GeneralSettingsModel>['bindAddress'];
  port: PendingSection<GeneralSettingsModel>['port'];
  urlBase: PendingSection<GeneralSettingsModel>['urlBase'];
  instanceName: PendingSection<GeneralSettingsModel>['instanceName'];
  applicationUrl: PendingSection<GeneralSettingsModel>['applicationUrl'];
  enableSsl: PendingSection<GeneralSettingsModel>['enableSsl'];
  sslPort: PendingSection<GeneralSettingsModel>['sslPort'];
  sslKeyPath: PendingSection<GeneralSettingsModel>['sslKeyPath'];
  sslCertPath: PendingSection<GeneralSettingsModel>['sslCertPath'];
  sslCertPassword: PendingSection<GeneralSettingsModel>['sslCertPassword'];
  launchBrowser: PendingSection<GeneralSettingsModel>['launchBrowser'];
  onInputChange: (change: InputChanged) => void;
}

function HostSettings({
  bindAddress,
  port,
  urlBase,
  instanceName,
  applicationUrl,
  enableSsl,
  sslPort,
  sslCertPath,
  sslKeyPath,
  sslCertPassword,
  launchBrowser,
  onInputChange,
}: HostSettingsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();
  const isWindowsService = useIsWindowsService();

  return (
    <FieldSet legend={translate('Host')} caption={translate('HostCaption')}>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('BindAddress')}</FormLabel>
        <FormInputHelpText text={translate('BindAddressHelpText')} />
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="bindAddress"
          onChange={onInputChange}
          {...bindAddress}
        />
      </FormRow>
      <FormRow>
        <FormLabel>{translate('PortNumber')}</FormLabel>
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.NUMBER}
          name="port"
          min={1}
          max={65535}
          autocomplete="off"
          onChange={onInputChange}
          {...port}
        />
      </FormRow>
      <FormRow>
        <FormLabel>{translate('UrlBase')}</FormLabel>
        <FormInputHelpText text={translate('UrlBaseHelpText')} />
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="urlBase"
          onChange={onInputChange}
          {...urlBase}
        />
      </FormRow>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('InstanceName')}</FormLabel>
        <FormInputHelpText text={translate('InstanceNameHelpText')} />
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="instanceName"
          onChange={onInputChange}
          {...instanceName}
        />
      </FormRow>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('ApplicationURL')}</FormLabel>
        <FormInputHelpText text={translate('ApplicationUrlHelpText')} />
        <FormInput
          type={inputTypes.TEXT}
          name="applicationUrl"
          onChange={onInputChange}
          {...applicationUrl}
        />
      </FormRow>
      <FormRow
        advancedSettings={showAdvancedSettings}
        isAdvanced={true}
        size={sizes.MEDIUM}
      >
        <FormLabel>{translate('EnableSsl')}</FormLabel>
        <FormInputHelpText text={translate('EnableSslHelpText')} />
        <FormInput
          type={inputTypes.CHECK}
          name="enableSsl"
          onChange={onInputChange}
          {...enableSsl}
        />
      </FormRow>
      {enableSsl.value ? (
        <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
          <FormLabel>{translate('SslPort')}</FormLabel>
          <FormInputHelpText
            text={translate('RestartRequiredHelpTextWarning')}
            isWarning={true}
          />
          <FormInput
            type={inputTypes.NUMBER}
            name="sslPort"
            min={1}
            max={65535}
            onChange={onInputChange}
            {...sslPort}
          />
        </FormRow>
      ) : null}
      {enableSsl.value ? (
        <>
          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SslCertPath')}</FormLabel>
            <FormInputHelpText text={translate('SslCertPathHelpText')} />
            <FormInputHelpText
              text={translate('RestartRequiredHelpTextWarning')}
              isWarning={true}
            />
            <FormInput
              type={inputTypes.TEXT}
              name="sslCertPath"
              onChange={onInputChange}
              {...sslCertPath}
            />
          </FormRow>

          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SslKeyPath')}</FormLabel>
            <FormInputHelpText text={translate('SslKeyPathHelpText')} />
            <FormInputHelpText
              text={translate('RestartRequiredHelpTextWarning')}
              isWarning={true}
            />
            <FormInput
              type={inputTypes.TEXT}
              name="sslKeyPath"
              onChange={onInputChange}
              {...sslKeyPath}
            />
          </FormRow>

          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SslCertPassword')}</FormLabel>
            <FormInputHelpText text={translate('SslCertPasswordHelpText')} />
            <FormInputHelpText
              text={translate('RestartRequiredHelpTextWarning')}
              isWarning={true}
            />
            <FormInput
              type={inputTypes.PASSWORD}
              name="sslCertPassword"
              onChange={onInputChange}
              {...sslCertPassword}
            />
          </FormRow>
        </>
      ) : null}
      {isWindowsService ? null : (
        <FormRow size={sizes.MEDIUM}>
          <FormLabel>{translate('OpenBrowserOnStart')}</FormLabel>
          <FormInputHelpText text={translate('OpenBrowserOnStartHelpText')} />
          <FormInput
            type={inputTypes.CHECK}
            name="launchBrowser"
            onChange={onInputChange}
            {...launchBrowser}
          />
        </FormRow>
      )}
    </FieldSet>
  );
}

export default HostSettings;
