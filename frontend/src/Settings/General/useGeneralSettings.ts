import {
  useManageSettings,
  useSaveSettings,
  useSettings,
} from 'Settings/useSettings';

export type UpdateMechanism =
  | 'builtIn'
  | 'script'
  | 'external'
  | 'apt'
  | 'docker';

export interface GeneralSettingsModel {
  bindAddress: string;
  port: number;
  sslPort: number;
  enableSsl: boolean;
  launchBrowser: boolean;
  authenticationMethod: string;
  authenticationRequired: string;
  analyticsEnabled: boolean;
  username: string;
  password: string;
  passwordConfirmation: string;
  logLevel: string;
  logSizeLimit: number;
  consoleLogLevel: string;
  branch: string;
  apiKey: string;
  sslCertPath: string;
  sslKeyPath: string;
  sslCertPassword: string;
  urlBase: string;
  instanceName: string;
  applicationUrl: string;
  updateAutomatically: boolean;
  updateMechanism: UpdateMechanism;
  updateScriptPath: string;
  proxyEnabled: boolean;
  proxyType: string;
  proxyHostname: string;
  proxyPort: number;
  proxyUsername: string;
  proxyPassword: string;
  proxyBypassFilter: string;
  proxyBypassLocalAddresses: boolean;
  certificateValidation: string;
  backupFolder: string;
  backupInterval: number;
  backupRetention: number;
  id: number;
}

const PATH = '/settings/general';

export const useGeneralSettings = () => {
  return useSettings<GeneralSettingsModel>(PATH);
};

export const useManageGeneralSettings = () => {
  return useManageSettings<GeneralSettingsModel>(PATH);
};

export const useSaveGeneralSettings = () => {
  return useSaveSettings<GeneralSettingsModel>(PATH);
};
