import { create } from 'zustand';
import { useShallow } from 'zustand/react/shallow';
import ModelBase from './ModelBase';

export type MessageType = 'error' | 'info' | 'success' | 'warning';

export interface Message extends ModelBase {
  hideAfter: number;
  message: string;
  name: string;
  type: MessageType;
}

interface MessagesState {
  messages: Message[];
}

const useMessagesStore = create<MessagesState>()(() => ({
  messages: [],
}));

export const useMessages = () => {
  return useMessagesStore(useShallow((state) => state.messages));
};

export const getMessages = () => {
  return useMessagesStore.getState().messages;
};

export const showMessage = (payload: Message) => {
  useMessagesStore.setState((state) => {
    const messages = [...state.messages];
    const index = messages.findIndex((item) => item.id === payload.id);

    if (index >= 0) {
      const item = messages[index];
      messages.splice(index, 1, { ...item, ...payload });
    } else {
      messages.push({ ...payload });
    }

    return {
      messages,
    };
  });
};

export const hideMessage = ({ id }: { id: string | number }) => {
  useMessagesStore.setState((state) => {
    const messages = state.messages.filter((item) => item.id !== id);

    return {
      messages,
    };
  });
};
