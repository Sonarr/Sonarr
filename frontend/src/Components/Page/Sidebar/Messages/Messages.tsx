import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { Message as MessageModel } from 'App/State/MessagesAppState';
import Message from './Message';
import styles from './Messages.css';

function Messages() {
  const items = useSelector((state: AppState) => state.app.messages.items);

  const messages = useMemo(() => {
    return items.reduce<MessageModel[]>((acc, item) => {
      acc.unshift(item);

      return acc;
    }, []);
  }, [items]);

  return (
    <div className={styles.messages}>
      {messages.map((message) => {
        return <Message key={message.id} {...message} />;
      })}
    </div>
  );
}

export default Messages;
