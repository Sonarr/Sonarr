interface SystemStatus {
  appData: string;
  appName: string;
  authentication: string;
  branch: string;
  buildTime: string;
  databaseVersion: string;
  databaseType: string;
  instanceName: string;
  isAdmin: boolean;
  isDebug: boolean;
  isContainerized: boolean;
  isLinux: boolean;
  isNetCore: boolean;
  isOsx: boolean;
  isProduction: boolean;
  isUserInteractive: boolean;
  isWindows: boolean;
  migrationVersion: number;
  mode: string;
  osName: string;
  osVersion: string;
  packageAuthor: string;
  packageUpdateMechanism: string;
  packageUpdateMechanismMessage: string;
  packageVersion: string;
  runtimeName: string;
  runtimeVersion: string;
  sqliteVersion: string;
  startTime: string;
  startupPath: string;
  urlBase: string;
  version: string;
}

export default SystemStatus;
