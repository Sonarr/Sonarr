import anySignal from './anySignal';

export class ApiError extends Error {
  public statusCode: number;
  public statusText: string;
  public statusBody?: ApiErrorResponse;

  public constructor(
    path: string,
    statusCode: number,
    statusText: string,
    statusBody?: ApiErrorResponse
  ) {
    super(`Request Error: (${statusCode}) ${path}`);

    this.statusCode = statusCode;
    this.statusText = statusText;
    this.statusBody = statusBody;

    Object.setPrototypeOf(this, new.target.prototype);
  }
}

export interface ApiErrorResponse {
  message: string;
  details: string;
}

export interface FetchJsonOptions<TData> extends Omit<RequestInit, 'body'> {
  path: string;
  headers?: HeadersInit;
  body?: TData;
  timeout?: number;
}

export const urlBase = window.Sonarr.urlBase;
export const apiRoot = '/api/v5'; // window.Sonarr.apiRoot;

async function fetchJson<T, TData>({
  body,
  path,
  signal,
  timeout,
  ...options
}: FetchJsonOptions<TData>): Promise<T> {
  const abortController = new AbortController();

  let timeoutID: ReturnType<typeof setTimeout> | null = null;

  if (timeout) {
    timeoutID = setTimeout(() => {
      abortController.abort();
    }, timeout);
  }

  const response = await fetch(path, {
    ...options,
    body: body ? JSON.stringify(body) : undefined,
    headers: {
      ...options.headers,
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    signal: anySignal(abortController.signal, signal),
  });

  if (timeoutID) {
    clearTimeout(timeoutID);
  }

  if (!response.ok) {
    // eslint-disable-next-line init-declarations
    let body;

    try {
      body = (await response.json()) as ApiErrorResponse;
    } catch {
      throw new ApiError(path, response.status, response.statusText);
    }

    throw new ApiError(path, response.status, response.statusText, body);
  }

  return response.json() as T;
}

export default fetchJson;
