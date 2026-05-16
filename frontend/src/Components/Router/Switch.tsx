import React, { ReactNode } from 'react';
import { Routes } from 'react-router-dom';

interface SwitchProps {
  children: ReactNode;
}

function Switch({ children }: SwitchProps) {
  return <Routes>{children}</Routes>;
}

export default Switch;
