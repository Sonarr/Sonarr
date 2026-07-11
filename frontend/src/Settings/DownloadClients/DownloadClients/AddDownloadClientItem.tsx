import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { icons, sizes } from 'Helpers/Props';
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
    <div className={styles.downloadClient}>
      <Link
        className={styles.underlay}
        aria-label={translate('AddDownloadClientImplementation', {
          implementationName,
        })}
        onPress={handleDownloadClientSelect}
      />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          {hasPresets ? (
            <Menu className={styles.presetsMenu} alignMenu="right">
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset) => (
                  <AddDownloadClientPresetMenuItem
                    key={preset.name}
                    name={preset.name}
                    implementation={implementation}
                    implementationName={implementationName}
                    onPress={onDownloadClientSelect}
                  />
                ))}
              </MenuContent>
            </Menu>
          ) : null}

          <Link
            className={styles.infoLink}
            to={infoLink}
            title={translate('MoreInfo')}
            aria-label={translate('MoreInfo')}
          >
            <Icon name={icons.INFO} size={18} />
          </Link>
        </div>
      </div>
    </div>
  );
}

export default AddDownloadClientItem;
