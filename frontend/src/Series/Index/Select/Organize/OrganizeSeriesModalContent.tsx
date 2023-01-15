import { orderBy } from 'lodash';
import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RENAME_SERIES } from 'Commands/commandNames';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import styles from './OrganizeSeriesModalContent.css';

interface OrganizeSeriesModalContentProps {
  seriesIds: number[];
  onModalClose: () => void;
}

function OrganizeSeriesModalContent(props: OrganizeSeriesModalContentProps) {
  const { seriesIds, onModalClose } = props;

  const allSeries = useSelector(createAllSeriesSelector());
  const dispatch = useDispatch();

  const seriesTitles = useMemo(() => {
    const series = seriesIds.map((id) => {
      return allSeries.find((s) => s.id === id);
    });

    const sorted = orderBy(series, ['sortTitle']);

    return sorted.map((s) => s.title);
  }, [seriesIds, allSeries]);

  const onOrganizePress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RENAME_SERIES,
        seriesIds,
      })
    );

    onModalClose();
  }, [seriesIds, onModalClose, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Organize Selected Series</ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview a rename, select "Cancel", then select any series
          title and use the
          <Icon className={styles.renameIcon} name={icons.ORGANIZE} />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to organize all files in the{' '}
          {seriesTitles.length} selected series?
        </div>

        <ul>
          {seriesTitles.map((title) => {
            return <li key={title}>{title}</li>;
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>

        <Button kind={kinds.DANGER} onPress={onOrganizePress}>
          Organize
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default OrganizeSeriesModalContent;
