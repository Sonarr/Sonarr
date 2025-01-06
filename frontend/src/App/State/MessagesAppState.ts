import ModelBase from 'App/ModelBase';
import AppSectionState from 'App/State/AppSectionState';

export type MessageType = 'error' | 'info' | 'success' | 'warning';

export interface Message extends ModelBase {
  hideAfter: number;
  message: string;
  name: string;
  type: MessageType;
}

type MessagesAppState = AppSectionState<Message>;

export default MessagesAppState;
