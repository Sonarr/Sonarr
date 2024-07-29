import classNames from 'classnames';
import React from 'react';
import { Kind } from 'Helpers/Props/kinds';
import styles from './Alert.css';

interface AlertProps {
  className?: string;
  kind?: Extract<Kind, keyof typeof styles>;
  children: React.ReactNode;
}

function Alert(props: AlertProps) {
  const { className = styles.alert, kind = 'info', children } = props;

  return <div className={classNames(className, styles[kind])}>{children}</div>;
}

export default Alert;
