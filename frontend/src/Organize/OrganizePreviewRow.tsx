import React, { useCallback, useEffect } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import { CheckInputChanged } from 'typings/inputs';
import { OrganizePreviewModel } from './useOrganizePreview';
import styles from './OrganizePreviewRow.css';

interface OrganizePreviewRowProps {
  id: number;
  existingPath: string;
  newPath: string;
}

function OrganizePreviewRow({
  id,
  existingPath,
  newPath,
}: OrganizePreviewRowProps) {
  const { toggleSelected, useIsSelected } = useSelect<OrganizePreviewModel>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ value, shiftKey }: CheckInputChanged) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [id, toggleSelected]
  );

  useEffect(() => {
    toggleSelected({
      id,
      isSelected: true,
      shiftKey: false,
    });
  }, [id, toggleSelected]);

  return (
    <div className={styles.row}>
      <CheckInput
        containerClassName={styles.selectedContainer}
        name={id.toString()}
        value={isSelected}
        onChange={handleSelectedChange}
      />

      <div>
        <div>
          <Icon name={icons.SUBTRACT} kind={kinds.DANGER} />

          <span className={styles.path}>{existingPath}</span>
        </div>

        <div>
          <Icon name={icons.ADD} kind={kinds.SUCCESS} />

          <span className={styles.path}>{newPath}</span>
        </div>
      </div>
    </div>
  );
}

export default OrganizePreviewRow;
