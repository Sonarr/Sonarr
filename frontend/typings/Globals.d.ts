declare module '*.module.css';

interface Window {
  Sonarr: {
    apiKey: string;
    instanceName: string;
    theme: string;
    urlBase: string;
    version: string;
    isProduction: boolean;
  };
}
