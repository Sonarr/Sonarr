import React, { useMemo } from 'react';
import { Message as MessageModel, useMessages } from 'App/messagesStore';
import Message from './Message';
import styles from './Messages.css';

function Messages() {
  const items = useMessages();

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
