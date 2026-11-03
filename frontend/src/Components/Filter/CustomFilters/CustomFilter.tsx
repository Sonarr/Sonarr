import React, { useCallback, useEffect, useState } from 'react';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { useDeleteCustomFilter } from 'Filters/useCustomFilters';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './CustomFilter.module.css';

interface CustomFilterProps {
  id: number;
  label: string;
  dispatchSetFilter: (payload: { selectedFilterKey: string | number }) => void;
  onEditPress: (id: number) => void;
}

function CustomFilter({
  id,
  label,
  dispatchSetFilter,
  onEditPress,
}: CustomFilterProps) {
  const { deleteCustomFilter, isDeleting, deleteError } =
    useDeleteCustomFilter(id);
  const wasDeleting = usePrevious(isDeleting);
  const [isDeletingInternal, setIsDeletingInternal] = useState(false);
  const editLabel = translate('EditCustomFilterName', { name: label });
  const removeLabel = translate('RemoveCustomFilterName', { name: label });

  const handleEditPress = useCallback(() => {
    onEditPress(id);
  }, [id, onEditPress]);

  const handleRemovePress = useCallback(() => {
    setIsDeletingInternal(true);

    deleteCustomFilter();
  }, [deleteCustomFilter]);

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
  }, [isDeletingInternal, dispatchSetFilter]);

  return (
    <div className={styles.customFilter}>
      <div className={styles.label}>{label}</div>

      <div className={styles.actions}>
        <IconButton
          name={icons.EDIT}
          title={editLabel}
          aria-label={editLabel}
          onPress={handleEditPress}
        />

        <SpinnerIconButton
          title={removeLabel}
          aria-label={removeLabel}
          name={icons.REMOVE}
          isSpinning={isDeleting}
          onPress={handleRemovePress}
        />
      </div>
    </div>
  );
}

export default CustomFilter;
