import { Error } from './AppSectionState';

interface OAuthAppState {
  authorizing: boolean;
  result: Record<string, unknown> | null;
  error: Error;
}

export default OAuthAppState;
