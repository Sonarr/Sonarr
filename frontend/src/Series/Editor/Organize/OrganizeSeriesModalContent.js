import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import styles from './OrganizeSeriesModalContent.css';

function OrganizeSeriesModalContent(props) {
  const {
    seriesTitles,
    onModalClose,
    onOrganizeSeriesPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Organize Selected Series
      </ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview a rename, select "Cancel", then select any series title and use the
          <Icon
            className={styles.renameIcon}
            name={icons.ORGANIZE}
          />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to organize all files in the {seriesTitles.length} selected series?
        </div>

        <ul>
          {
            seriesTitles.map((title) => {
              return (
                <li key={title}>
                  {title}
                </li>
              );
            })
          }
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          Cancel
        </Button>

        <Button
          kind={kinds.DANGER}
          onPress={onOrganizeSeriesPress}
        >
          Organize
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

OrganizeSeriesModalContent.propTypes = {
  seriesTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onOrganizeSeriesPress: PropTypes.func.isRequired
};

export default OrganizeSeriesModalContent;
