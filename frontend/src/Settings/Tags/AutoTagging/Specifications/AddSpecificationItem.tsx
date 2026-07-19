import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import { AutoTaggingSpecification } from '../useAutoTaggings';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

interface AddSpecificationItemProps {
  implementation: string;
  implementationName: string;
  infoLink?: string;
  presets?: AutoTaggingSpecification[];
  onSpecificationSelect: ({
    implementation,
  }: {
    implementation: string;
  }) => void;
}

export default function AddSpecificationItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onSpecificationSelect,
}: AddSpecificationItemProps) {
  const handleSpecificationSelect = useCallback(() => {
    onSpecificationSelect({ implementation });
  }, [implementation, onSpecificationSelect]);

  const hasPresets = !!presets && !!presets.length;

  return (
    <Card
      className={styles.specification}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('AddConditionImplementation', {
        implementationName,
      })}
      onPress={handleSpecificationSelect}
    >
      <div className={styles.name}>{implementationName}</div>

      <div className={styles.actions}>
        {hasPresets ? (
          <span>
            <Button size={sizes.SMALL} onPress={handleSpecificationSelect}>
              {translate('Custom')}
            </Button>

            <Menu className={styles.presetsMenu}>
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset, index) => {
                  return (
                    <AddSpecificationPresetMenuItem
                      key={index}
                      name={preset.name}
                      implementation={implementation}
                      onPress={handleSpecificationSelect}
                    />
                  );
                })}
              </MenuContent>
            </Menu>
          </span>
        ) : null}

        {infoLink ? (
          <Button to={infoLink} size={sizes.SMALL}>
            {translate('MoreInfo')}
          </Button>
        ) : null}
      </div>
    </Card>
  );
}
