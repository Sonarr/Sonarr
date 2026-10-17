import React from 'react';
import { StatusMessage } from 'typings/Queue';
import styles from './QueueStatus.module.css';

interface QueueStatusMessagesProps {
  statusMessages: StatusMessage[];
}

function QueueStatusMessages({ statusMessages }: QueueStatusMessagesProps) {
  return (
    <div>
      {statusMessages.map(({ title, messages }) => {
        return (
          <div
            key={title}
            className={messages.length ? undefined : styles.noMessages}
          >
            {title}
            <ul>
              {messages.map((message) => {
                return <li key={message}>{message}</li>;
              })}
            </ul>
          </div>
        );
      })}
    </div>
  );
}

export default QueueStatusMessages;
