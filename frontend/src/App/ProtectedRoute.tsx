import React, { useCallback } from 'react';
import { Redirect, Route } from 'react-router-dom';

function ProtectedRoute({ component: Component, allowedRoles, ...rest }) {
  const userRole = window.sonarr.role;

  const renderComponent = useCallback(
    (props) => {
      return allowedRoles.includes(userRole) ? (
        <Component {...props} />
      ) : (
        <Redirect to="/" /> // Redirect to home or any other page if not authorized
      );
    },
    [allowedRoles, userRole, Component]
  );

  return <Route {...rest} render={renderComponent} />;
}

export default ProtectedRoute;
