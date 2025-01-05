import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { CustomFormatAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import {
  cloneCustomFormat,
  fetchCustomFormats,
} from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import CustomFormatModel from 'typings/CustomFormat';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import CustomFormat from './CustomFormat';
import EditCustomFormatModal from './EditCustomFormatModal';
import styles from './CustomFormats.css';

function CustomFormats() {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, isDeleting, items } = useSelector(
    createSortedSectionSelector<CustomFormatModel, CustomFormatAppState>(
      'settings.customFormats',
      sortByProp('name')
    )
  );

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [clonedId, setClonedId] = useState<number>();

  const handleAddCustomFormatPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const handleCloneCustomFormatPress = useCallback(
    (id: number) => {
      dispatch(cloneCustomFormat({ id }));

      setIsEditModalOpen(true);
      setClonedId(id);
    },
    [dispatch]
  );

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
    setClonedId(undefined);
  }, []);

  useEffect(() => {
    dispatch(fetchCustomFormats());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('CustomFormats')}>
      <PageSectionContent
        errorMessage={translate('CustomFormatsLoadError')}
        isFetching={isFetching}
        isPopulated={isPopulated}
        error={error}
      >
        <div className={styles.customFormats}>
          {items.map((item) => {
            return (
              <CustomFormat
                key={item.id}
                {...item}
                isDeleting={isDeleting}
                onCloneCustomFormatPress={handleCloneCustomFormatPress}
              />
            );
          })}

          <Card
            className={styles.addCustomFormat}
            onPress={handleAddCustomFormatPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <EditCustomFormatModal
          isOpen={isEditModalOpen}
          clonedId={clonedId}
          onModalClose={handleEditModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default CustomFormats;
