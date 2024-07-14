interface SystemStatus {
  appData: string;
  appName: string;
  authentication: string;
  branch: string;
  buildTime: string;
  instanceName: string;
  isAdmin: boolean;
  isDebug: boolean;
  isDocker: boolean;
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
  packageUpdateMechanism: string;
  packageUpdateMechanismMessage: string;
  runtimeName: string;
  runtimeVersion: string;
  sqliteVersion: string;
  startTime: string;
  startupPath: string;
  urlBase: string;
  version: string;
}

export default SystemStatus;
