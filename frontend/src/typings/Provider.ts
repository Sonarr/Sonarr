import ModelBase from 'App/ModelBase';
import { Kind } from 'Helpers/Props/kinds';
import Field from './Field';

export interface ProviderMessage {
  message: string;
  type: Extract<Kind, 'info' | 'error' | 'warning'>;
}

interface Provider extends ModelBase {
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  message: ProviderMessage;
}

export default Provider;
