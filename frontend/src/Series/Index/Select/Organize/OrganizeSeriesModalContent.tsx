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
import Series from 'Series/Series';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import translate from 'Utilities/String/translate';
import styles from './OrganizeSeriesModalContent.css';

interface OrganizeSeriesModalContentProps {
  seriesIds: number[];
  onModalClose: () => void;
}

function OrganizeSeriesModalContent(props: OrganizeSeriesModalContentProps) {
  const { seriesIds, onModalClose } = props;

  const allSeries: Series[] = useSelector(createAllSeriesSelector());
  const dispatch = useDispatch();

  const seriesTitles = useMemo(() => {
    const series = seriesIds.reduce((acc: Series[], id) => {
      const s = allSeries.find((s) => s.id === id);

      if (s) {
        acc.push(s);
      }

      return acc;
    }, []);

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
      <ModalHeader>
        {translate('OrganizeSelectedSeriesModalHeader')}
      </ModalHeader>

      <ModalBody>
        <Alert>
          {translate('OrganizeSelectedSeriesModalAlert')}
          <Icon className={styles.renameIcon} name={icons.ORGANIZE} />
        </Alert>

        <div className={styles.message}>
          {translate('OrganizeSelectedSeriesModalConfirmation', {
            count: seriesTitles.length,
          })}
        </div>

        <ul>
          {seriesTitles.map((title) => {
            return <li key={title}>{title}</li>;
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onOrganizePress}>
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default OrganizeSeriesModalContent;
