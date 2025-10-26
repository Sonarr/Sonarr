import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/Select/SelectContext';
import SeriesTagList from 'Components/SeriesTagList';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { createQualityProfileSelectorForHook } from 'Store/Selectors/createQualityProfileSelector';
import ImportList from 'typings/ImportList';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './ManageImportListsModalRow.css';

interface ManageImportListsModalRowProps {
  id: number;
  name: string;
  rootFolderPath: string;
  qualityProfileId: number;
  implementation: string;
  tags: number[];
  enableAutomaticAdd: boolean;
  columns: Column[];
}

function ManageImportListsModalRow(props: ManageImportListsModalRowProps) {
  const {
    id,
    name,
    rootFolderPath,
    qualityProfileId,
    implementation,
    enableAutomaticAdd,
    tags,
  } = props;

  const { toggleSelected, useIsSelected } = useSelect<ImportList>();
  const isSelected = useIsSelected(id);

  const qualityProfile = useSelector(
    createQualityProfileSelectorForHook(qualityProfileId)
  );

  const onSelectedChangeWrapper = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.implementation}>
        {implementation}
      </TableRowCell>

      <TableRowCell className={styles.qualityProfileId}>
        {qualityProfile?.name ?? translate('None')}
      </TableRowCell>

      <TableRowCell className={styles.rootFolderPath}>
        {rootFolderPath}
      </TableRowCell>

      <TableRowCell className={styles.enableAutomaticAdd}>
        {enableAutomaticAdd ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.tags}>
        <SeriesTagList tags={tags} />
      </TableRowCell>
    </TableRow>
  );
}

export default ManageImportListsModalRow;
