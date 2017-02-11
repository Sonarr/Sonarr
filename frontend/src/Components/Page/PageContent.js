import PropTypes from 'prop-types';
import React from 'react';
import DocumentTitle from 'react-document-title';
import styles from './PageContent.css';

function PageContent(props) {
  const {
    className,
    title,
    children
  } = props;

  return (
    <DocumentTitle title={title ? `${title} - Sonarr` : 'Sonarr'}>
      <div className={className}>
        {children}
      </div>
    </DocumentTitle>
  );
}

PageContent.propTypes = {
  className: PropTypes.string,
  title: PropTypes.string,
  children: PropTypes.node.isRequired
};

PageContent.defaultProps = {
  className: styles.content
};

export default PageContent;
