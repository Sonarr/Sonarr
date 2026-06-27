import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddDownloadClientPresetMenuItem from './AddDownloadClientPresetMenuItem';
import { DownloadClientModel } from './useDownloadClients';
import styles from './AddDownloadClientItem.css';

interface AddDownloadClientItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: DownloadClientModel[];
  onDownloadClientSelect: (selectedSchema: SelectedSchema) => void;
}

function AddDownloadClientItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onDownloadClientSelect,
}: AddDownloadClientItemProps) {
  const hasPresets = !!presets && !!presets.length;

  const handleDownloadClientSelect = useCallback(() => {
    onDownloadClientSelect({ implementation, implementationName });
  }, [implementation, implementationName, onDownloadClientSelect]);

  return (
    <Card
      className={styles.downloadClient}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('AddDownloadClientImplementation', {
        implementationName,
      })}
      onPress={handleDownloadClientSelect}
    >
      <div className={styles.name}>{implementationName}</div>

      <div className={styles.actions}>
        {hasPresets ? (
          <span>
            <Button size={sizes.SMALL} onPress={handleDownloadClientSelect}>
              {translate('Custom')}
            </Button>

            <Menu className={styles.presetsMenu}>
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset) => {
                  return (
                    <AddDownloadClientPresetMenuItem
                      key={preset.name}
                      name={preset.name}
                      implementation={implementation}
                      implementationName={implementationName}
                      onPress={onDownloadClientSelect}
                    />
                  );
                })}
              </MenuContent>
            </Menu>
          </span>
        ) : null}

        <Button to={infoLink} size={sizes.SMALL}>
          {translate('MoreInfo')}
        </Button>
      </div>
    </Card>
  );
}

export default AddDownloadClientItem;
