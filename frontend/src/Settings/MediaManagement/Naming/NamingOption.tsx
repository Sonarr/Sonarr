import classNames from 'classnames';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, tooltipPositions } from 'Helpers/Props';
import { Size } from 'Helpers/Props/sizes';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import styles from './NamingOption.css';

interface NamingOptionProps {
  token: string;
  tokenSeparator: TokenSeparator;
  example: string;
  tokenCase: TokenCase;
  isFullFilename?: boolean;
  notes?: string[];
  size?: Extract<Size, keyof typeof styles>;
  onPress: ({
    isFullFilename,
    tokenValue,
  }: {
    isFullFilename: boolean;
    tokenValue: string;
  }) => void;
}

function NamingOption(props: NamingOptionProps) {
  const {
    token,
    tokenSeparator,
    example,
    tokenCase,
    isFullFilename = false,
    notes,
    size = 'small',
    onPress,
  } = props;

  const handlePress = useCallback(() => {
    let tokenValue = token;

    tokenValue = tokenValue.replace(/ /g, tokenSeparator);

    if (tokenCase === 'lower') {
      tokenValue = token.toLowerCase();
    } else if (tokenCase === 'upper') {
      tokenValue = token.toUpperCase();
    }

    onPress({ isFullFilename, tokenValue });
  }, [token, tokenCase, tokenSeparator, isFullFilename, onPress]);

  return (
    <div
      className={classNames(
        styles.option,
        styles[size],
        styles[tokenCase],
        isFullFilename && styles.isFullFilename
      )}
    >
      <Link className={styles.insert} onPress={handlePress}>
        <div className={styles.token}>
          {token.replace(/ /g, tokenSeparator)}
        </div>

        <div className={styles.example}>
          {example.replace(/ /g, tokenSeparator)}
        </div>

        {isFullFilename ? null : (
          <div className={styles.action}>
            <Icon name={icons.ADD} size={13} />
          </div>
        )}
      </Link>

      {notes?.length ? (
        <Tooltip
          className={styles.note}
          anchor={<Icon name={icons.INFO} size={13} />}
          tooltip={
            <div className={styles.noteBody}>
              {notes.map((note, index) => (
                <p key={index}>
                  <InlineMarkdown data={note} />
                </p>
              ))}
            </div>
          }
          position={tooltipPositions.TOP}
        />
      ) : null}
    </div>
  );
}

export default NamingOption;
