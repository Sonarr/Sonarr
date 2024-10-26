import ModelBase from 'App/ModelBase';
import Field from './Field';

interface Notification extends ModelBase {
  enable: boolean;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  tags: number[];
}

export default Notification;
