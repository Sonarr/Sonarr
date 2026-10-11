declare module '*.module.css';
declare module 'moment/min/locales';

interface Window {
  Sonarr: {
    apiKey: string;
    apiRoot: string;
    instanceName: string;
    theme: string;
    urlBase: string;
    version: string;
    isProduction: boolean;
  };
}
