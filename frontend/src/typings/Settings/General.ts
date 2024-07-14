export type UpdateMechanism =
  | 'builtIn'
  | 'script'
  | 'external'
  | 'apt'
  | 'docker';

export default interface General {
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
  consoleLogLevel: string;
  branch: string;
  apiKey: string;
  sslCertPath: string;
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
