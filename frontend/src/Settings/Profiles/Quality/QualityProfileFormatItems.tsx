import React, { useMemo } from 'react';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import useDebounce from 'Helpers/Hooks/useDebounce';
import { QualityProfileFormatItem as QualityProfileFormatItemModel } from 'Settings/Profiles/Quality/useQualityProfiles';
import { Failure } from 'typings/pending';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItem from './QualityProfileFormatItem';
import styles from './QualityProfileFormatItems.module.css';

interface QualityProfileFormatItemsProps {
  profileFormatItems: QualityProfileFormatItemModel[];
  errors?: Failure[];
  warnings?: Failure[];
  onQualityProfileFormatItemScoreChange: (
    formatId: number,
    score: number
  ) => void;
}

function QualityProfileFormatItems({
  profileFormatItems,
  errors = [],
  warnings = [],
  onQualityProfileFormatItemScoreChange,
}: QualityProfileFormatItemsProps) {
  const sortFormatItems = useDebounce(profileFormatItems, 1000);

  const order = useMemo(() => {
    const items = profileFormatItems.reduce<Record<number, number>>(
      (acc, cur, index) => {
        acc[cur.format] = index;
        return acc;
      },
      {}
    );

    const sortScores = sortFormatItems.reduce<Record<number, number>>(
      (acc, cur) => {
        acc[cur.format] = cur.score;
        return acc;
      },
      {}
    );

    return [...profileFormatItems]
      .sort((a, b) => {
        const aScore = a.format in sortScores ? sortScores[a.format] : a.score;
        const bScore = b.format in sortScores ? sortScores[b.format] : b.score;

        if (bScore !== aScore) {
          return bScore - aScore;
        }

        return a.name.localeCompare(b.name, undefined, { numeric: true });
      })
      .map((x) => items[x.format]);
  }, [profileFormatItems, sortFormatItems]);

  if (profileFormatItems.length < 1) {
    return (
      <InlineMarkdown
        className={styles.addCustomFormatMessage}
        data={translate('WantMoreControlAddACustomFormat')}
      />
    );
  }

  return (
    <div>
      <FormInputHelpText text={translate('CustomFormatHelpText')} />

      {errors.map((error, index) => {
        return (
          <FormInputHelpText
            key={index}
            text={error.message}
            isError={true}
            isCheckInput={false}
          />
        );
      })}

      {warnings.map((warning, index) => {
        return (
          <FormInputHelpText
            key={index}
            text={warning.message}
            isWarning={true}
            isCheckInput={false}
          />
        );
      })}

      <div className={styles.formats}>
        <div className={styles.headerContainer}>
          <div className={styles.headerTitle}>{translate('CustomFormat')}</div>
          <div className={styles.headerScore}>{translate('Score')}</div>
        </div>

        {order.map((index) => {
          const { format, name, score } = profileFormatItems[index];
          return (
            <QualityProfileFormatItem
              key={format}
              formatId={format}
              name={name}
              score={score}
              onScoreChange={onQualityProfileFormatItemScoreChange}
            />
          );
        })}
      </div>
    </div>
  );
}

export default QualityProfileFormatItems;
