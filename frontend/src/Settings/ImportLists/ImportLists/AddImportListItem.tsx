import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddImportListPresetMenuItem from './AddImportListPresetMenuItem';
import { ImportListModel } from './useImportLists';
import styles from './AddImportListItem.css';

interface AddImportListItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: ImportListModel[];
  onImportListSelect: (selectedSchema: SelectedSchema) => void;
}

function AddImportListItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onImportListSelect,
}: AddImportListItemProps) {
  const hasPresets = !!(presets && presets.length);

  const handleImportListSelect = useCallback(() => {
    onImportListSelect({ implementation, implementationName });
  }, [implementation, implementationName, onImportListSelect]);

  return (
    <Card
      className={styles.list}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('AddImportListImplementation', {
        implementationName,
      })}
      onPress={handleImportListSelect}
    >
      <div className={styles.name}>{implementationName}</div>

      <div className={styles.actions}>
        {hasPresets && (
          <span>
            <Button size={sizes.SMALL} onPress={handleImportListSelect}>
              {translate('Custom')}
            </Button>

            <Menu className={styles.presetsMenu}>
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset) => {
                  return (
                    <AddImportListPresetMenuItem
                      key={preset.name}
                      name={preset.name}
                      implementation={implementation}
                      implementationName={implementationName}
                      onPress={onImportListSelect}
                    />
                  );
                })}
              </MenuContent>
            </Menu>
          </span>
        )}

        <Button to={infoLink} size={sizes.SMALL}>
          {translate('MoreInfo')}
        </Button>
      </div>
    </Card>
  );
}

export default AddImportListItem;
