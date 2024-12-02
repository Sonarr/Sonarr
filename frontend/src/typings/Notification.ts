import Provider from './Provider';

interface Notification extends Provider {
  enable: boolean;
  tags: number[];
}

export default Notification;
