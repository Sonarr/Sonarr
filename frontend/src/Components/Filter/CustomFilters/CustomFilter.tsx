import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { Error } from 'App/State/AppSectionState';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import { deleteCustomFilter } from 'Store/Actions/customFilterActions';
import translate from 'Utilities/String/translate';
import styles from './CustomFilter.css';

interface CustomFilterProps {
  id: number;
  label: string;
  isDeleting: boolean;
  deleteError?: Error;
  dispatchSetFilter: (payload: { selectedFilterKey: string | number }) => void;
  onEditPress: (id: number) => void;
}

function CustomFilter({
  id,
  label,
  isDeleting,
  deleteError,
  dispatchSetFilter,
  onEditPress,
}: CustomFilterProps) {
  const dispatch = useDispatch();
  const wasDeleting = usePrevious(isDeleting);
  const [isDeletingInternal, setIsDeletingInternal] = useState(false);

  const handleEditPress = useCallback(() => {
    onEditPress(id);
  }, [id, onEditPress]);

  const handleRemovePress = useCallback(() => {
    setIsDeletingInternal(true);

    dispatch(deleteCustomFilter({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (wasDeleting && !isDeleting && isDeletingInternal && deleteError) {
      setIsDeletingInternal(false);
    }
  }, [isDeleting, isDeletingInternal, wasDeleting, deleteError]);

  useEffect(() => {
    return () => {
      // Assume that delete and then unmounting means the deletion was successful.
      // Moving this check to an ancestor would be more accurate, but would have
      // more boilerplate.
      if (isDeletingInternal) {
        dispatchSetFilter({ selectedFilterKey: 'all' });
      }
    };
  }, [isDeletingInternal, dispatchSetFilter, dispatch]);

  return (
    <div className={styles.customFilter}>
      <div className={styles.label}>{label}</div>

      <div className={styles.actions}>
        <IconButton name={icons.EDIT} onPress={handleEditPress} />

        <SpinnerIconButton
          title={translate('RemoveFilter')}
          name={icons.REMOVE}
          isSpinning={isDeleting}
          onPress={handleRemovePress}
        />
      </div>
    </div>
  );
}

export default CustomFilter;
