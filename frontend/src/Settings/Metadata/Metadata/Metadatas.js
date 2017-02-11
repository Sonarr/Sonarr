import PropTypes from 'prop-types';
import React from 'react';
import sortByName from 'Utilities/Array/sortByName';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import Metadata from './Metadata';
import styles from './Metadatas.css';

function Metadatas(props) {
  const {
    items,
    ...otherProps
  } = props;

  return (
    <FieldSet
      legend="Metadata"
    >
      <PageSectionContent
        errorMessage="Unable to load Metadata"
        {...otherProps}
      >
        <div className={styles.metadatas}>
          {
            items.sort(sortByName).map((item) => {
              return (
                <Metadata
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

Metadatas.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Metadatas;
