import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { kinds } from 'Helpers/Props';
import TagConnector from './TagConnector';
import styles from './Tags.css';

function Tags(props) {
  const {
    items,
    ...otherProps
  } = props;

  if (!items.length) {
    return (
      <Alert kind={kinds.INFO}>
        No tags have been added yet
      </Alert>
    );
  }

  return (
    <FieldSet
      legend="Tags"
    >
      <PageSectionContent
        errorMessage="Unable to load Tags"
        {...otherProps}
      >
        <div className={styles.tags}>
          {
            items.map((item) => {
              return (
                <TagConnector
                  key={item.id}
                  {...item}
                />
              );
            })
          }
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

Tags.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Tags;
