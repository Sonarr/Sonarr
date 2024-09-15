import React, { ComponentType, useCallback } from 'react';
import {
  Redirect,
  Route,
  RouteComponentProps,
  RouteProps,
} from 'react-router-dom';
import { Role } from './Roles';

interface ProtectedRouteProps extends RouteProps {
  component: ComponentType<RouteComponentProps> | ComponentType;
  allowedRoles: Role[];
}

function ProtectedRoute({
  component: Component,
  allowedRoles,
  ...rest
}: ProtectedRouteProps) {
  const userRole: Role = window.Sonarr.role;

  const renderComponent = useCallback(
    (props: RouteComponentProps) => {
      return allowedRoles.includes(userRole) ? (
        <Component {...props} />
      ) : (
        <Redirect to="/" />
      );
    },
    [allowedRoles, userRole, Component]
  );

  return <Route {...rest} render={renderComponent} />;
}

export default ProtectedRoute;
