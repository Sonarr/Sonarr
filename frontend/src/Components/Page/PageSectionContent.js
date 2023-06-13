import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';

function PageSectionContent(props) {
  const {
    isFetching,
    isPopulated,
    error,
    errorMessage,
    children
  } = props;

  if (isFetching) {
    return (
      <LoadingIndicator />
    );
  } else if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{errorMessage}</Alert>
    );
  } else if (isPopulated && !error) {
    return (
      <div>{children}</div>
    );
  }

  return null;
}

PageSectionContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  errorMessage: PropTypes.string.isRequired,
  children: PropTypes.node.isRequired
};

export default PageSectionContent;
