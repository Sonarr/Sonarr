import { LogLevel } from '@microsoft/signalr';

export default class SignalRLogger {
  private _minimumLogLevel = LogLevel.Information;

  constructor(minimumLogLevel: LogLevel) {
    this._minimumLogLevel = minimumLogLevel;
  }

  public log(logLevel: LogLevel, message: string) {
    // see https://github.com/aspnet/AspNetCore/blob/21c9e2cc954c10719878839cd3f766aca5f57b34/src/SignalR/clients/ts/signalr/src/Utils.ts#L147
    if (logLevel >= this._minimumLogLevel) {
      switch (logLevel) {
        case LogLevel.Critical:
        case LogLevel.Error:
          console.error(
            `[signalR] ${LogLevel[logLevel]}: ${this._cleanse(message)}`
          );
          break;
        case LogLevel.Warning:
          console.warn(
            `[signalR] ${LogLevel[logLevel]}: ${this._cleanse(message)}`
          );
          break;
        case LogLevel.Information:
          console.info(
            `[signalR] ${LogLevel[logLevel]}: ${this._cleanse(message)}`
          );
          break;
        default:
          // console.debug only goes to attached debuggers in Node, so we use console.log for Trace and Debug
          console.log(
            `[signalR] ${LogLevel[logLevel]}: ${this._cleanse(message)}`
          );
          break;
      }
    }
  }

  private _cleanse(message: string) {
    const apikey = new RegExp(
      `access_token=${encodeURIComponent(window.Sonarr.apiKey)}`,
      'g'
    );
    return message.replace(apikey, 'access_token=(removed)');
  }
}
