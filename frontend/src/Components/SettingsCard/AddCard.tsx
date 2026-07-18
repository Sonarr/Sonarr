import React from 'react';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import styles from './SettingsCard.css';

interface AddCardProps {
  label: string;
  onPress: () => void;
}

function AddCard({ label, onPress }: AddCardProps) {
  return (
    <Card className={styles.addCard} aria-label={label} onPress={onPress}>
      <div className={styles.addCardCenter}>
        <Icon name={icons.ADD} size={20} />
      </div>

      <div className={styles.addCardLabel}>{label}</div>
    </Card>
  );
}

export default AddCard;
