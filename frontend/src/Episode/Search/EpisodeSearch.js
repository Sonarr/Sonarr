import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EpisodeSearch.css';

function EpisodeSearch(props) {
  const {
    onQuickSearchPress,
    onInteractiveSearchPress
  } = props;

  return (
    <div>
      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          size={sizes.LARGE}
          onPress={onQuickSearchPress}
        >
          <Icon
            className={styles.buttonIcon}
            name={icons.QUICK}
          />

          {translate('QuickSearch')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          kind={kinds.PRIMARY}
          size={sizes.LARGE}
          onPress={onInteractiveSearchPress}
        >
          <Icon
            className={styles.buttonIcon}
            name={icons.INTERACTIVE}
          />

          {translate('InteractiveSearch')}
        </Button>
      </div>
    </div>
  );
}

EpisodeSearch.propTypes = {
  onQuickSearchPress: PropTypes.func.isRequired,
  onInteractiveSearchPress: PropTypes.func.isRequired
};

export default EpisodeSearch;
