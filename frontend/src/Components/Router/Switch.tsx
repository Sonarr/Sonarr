import React, { Children, ReactElement, ReactNode } from 'react';
import { Switch as RouterSwitch } from 'react-router-dom';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';

interface ExtendedRoute {
  path: string;
  addUrlBase?: boolean;
}

interface SwitchProps {
  children: ReactNode;
}

function Switch({ children }: SwitchProps) {
  return (
    <RouterSwitch>
      {Children.map(children, (child) => {
        if (!React.isValidElement<ExtendedRoute>(child)) {
          return child;
        }

        const elementChild: ReactElement<ExtendedRoute> = child;

        const { path: childPath, addUrlBase = true } = elementChild.props;

        if (!childPath) {
          return child;
        }

        const path = addUrlBase ? getPathWithUrlBase(childPath) : childPath;

        return React.cloneElement(child, { path });
      })}
    </RouterSwitch>
  );
}

export default Switch;
