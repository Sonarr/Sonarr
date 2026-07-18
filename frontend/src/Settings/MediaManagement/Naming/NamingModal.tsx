import React, { useCallback, useState } from 'react';
import SelectInput, { SelectInputOption } from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { icons, sizes } from 'Helpers/Props';
import { Size } from 'Helpers/Props/sizes';
import translate from 'Utilities/String/translate';
import NamingOption from './NamingOption';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import { NamingSettingsModel } from './useNamingSettings';
import styles from './NamingModal.css';

interface Token {
  token: string;
  example: string;
  footNotes?: string;
  notes?: string[];
}

interface TokenGroup {
  legend: string;
  caption?: string;
  tokens: Token[];
  size?: Extract<Size, 'large'>;
  isFullFilename?: boolean;
}

function withFootNotes(
  tokens: Token[],
  notes: Record<string, string>
): Token[] {
  return tokens.map((token) =>
    token.footNotes
      ? {
          ...token,
          notes: token.footNotes
            .split(',')
            .map((id) => notes[id.trim()])
            .filter((note): note is string => note != null),
        }
      : token
  );
}

interface TokenFlags {
  season: boolean;
  episode: boolean;
  daily: boolean;
  anime: boolean;
  additional: boolean;
}

type SeparatorInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenSeparator;
};

type CaseInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenCase;
};

const separatorOptions: SeparatorInputOption[] = [
  {
    key: ' ',
    get value() {
      return `${translate('Space')} ( )`;
    },
  },
  {
    key: '.',
    get value() {
      return `${translate('Period')} (.)`;
    },
  },
  {
    key: '_',
    get value() {
      return `${translate('Underscore')} (_)`;
    },
  },
  {
    key: '-',
    get value() {
      return `${translate('Dash')} (-)`;
    },
  },
];

const caseOptions: CaseInputOption[] = [
  {
    key: 'title',
    get value() {
      return translate('DefaultCase');
    },
  },
  {
    key: 'lower',
    get value() {
      return translate('Lowercase');
    },
  },
  {
    key: 'upper',
    get value() {
      return translate('Uppercase');
    },
  },
];

const fileNameTokens = [
  {
    token:
      '{Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle} {Quality Full}',
    example:
      "The Series Title's! (2010) - S01E01 - Episode Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Series TitleYear} - {season:0}x{episode:00} - {Episode CleanTitle} {Quality Full}',
    example:
      "The Series Title's! (2010) - 1x01 - Episode Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Series.CleanTitleYear}.S{season:00}E{episode:00}.{Episode.CleanTitle}.{Quality.Full}',
    example: "The.Series.Title's!.2010.S01E01.Episode.Title.WEBDL-1080p.Proper",
  },
];

const fileNameDailyTokens = [
  {
    token:
      '{Series TitleYear} - {Air-Date} - {Episode CleanTitle} {Quality Full}',
    example:
      "The Series Title's! (2010) - 2013-10-30 - Episode Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Series.CleanTitleYear}.{Air.Date}.{Episode.CleanTitle}.{Quality.Full}',
    example:
      "The.Series.Title's!.2010.2013.10.30.Episode.Title.WEBDL-1080p.Proper",
  },
];

const fileNameAnimeTokens = [
  {
    token:
      '{Series TitleYear} - S{season:00}E{episode:00} - {absolute:000} - {Episode CleanTitle} {Quality Full}',
    example:
      "The Series Title's! (2010) - S01E01 - 001- Episode Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Series TitleYear} - {season:0}x{episode:00} - {absolute:000} - {Episode CleanTitle} {Quality Full}',
    example:
      "The Series Title's! (2010) - 1x01 - 001 - Episode Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Series.CleanTitleYear}.S{season:00}E{episode:00}.{absolute:000}.{Episode.CleanTitle}.{Quality.Full}',
    example:
      "The.Series.Title's!.2010.S01E01.001.Episode.Title.WEBDL-1080p.Proper",
  },
];

const seriesTokens = [
  { token: '{Series Title}', example: "The Series Title's!", footNotes: '1' },
  {
    token: '{Series CleanTitle}',
    example: "The Series Title's!",
    footNotes: '1',
  },
  {
    token: '{Series TitleYear}',
    example: "The Series Title's! (2010)",
    footNotes: '1',
  },
  {
    token: '{Series CleanTitleYear}',
    example: "The Series Title's! 2010",
    footNotes: '1',
  },
  {
    token: '{Series TitleWithoutYear}',
    example: "The Series Title's!",
    footNotes: '1',
  },
  {
    token: '{Series CleanTitleWithoutYear}',
    example: "The Series Title's!",
    footNotes: '1',
  },
  {
    token: '{Series TitleThe}',
    example: "Series Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Series CleanTitleThe}',
    example: "Series Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Series TitleTheYear}',
    example: "Series Title's!, The (2010)",
    footNotes: '1',
  },
  {
    token: '{Series CleanTitleTheYear}',
    example: "Series Title's!, The 2010",
    footNotes: '1',
  },
  {
    token: '{Series TitleTheWithoutYear}',
    example: "Series Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Series CleanTitleTheWithoutYear}',
    example: "Series Title's!, The",
    footNotes: '1',
  },
  { token: '{Series TitleFirstCharacter}', example: 'S', footNotes: '1' },
  { token: '{Series Year}', example: '2010' },
];

const seriesIdTokens = [
  { token: '{ImdbId}', example: 'tt12345' },
  { token: '{TvdbId}', example: '12345' },
  { token: '{TmdbId}', example: '11223' },
  { token: '{TvMazeId}', example: '54321' },
];

const seasonTokens = [
  { token: '{season:0}', example: '1' },
  { token: '{season:00}', example: '01' },
];

const episodeTokens = [
  { token: '{episode:0}', example: '1' },
  { token: '{episode:00}', example: '01' },
];

const airDateTokens = [
  { token: '{Air-Date}', example: '2016-03-20' },
  { token: '{Air Date}', example: '2016 03 20' },
];

const absoluteTokens = [
  { token: '{absolute:0}', example: '1' },
  { token: '{absolute:00}', example: '01' },
  { token: '{absolute:000}', example: '001' },
];

const episodeTitleTokens = [
  { token: '{Episode Title}', example: "Episode's Title", footNotes: '1' },
  { token: '{Episode CleanTitle}', example: 'Episodes Title', footNotes: '1' },
];

const qualityTokens = [
  { token: '{Quality Full}', example: 'WEBDL-1080p Proper' },
  { token: '{Quality Title}', example: 'WEBDL-1080p' },
];

const mediaInfoTokens = [
  { token: '{MediaInfo Simple}', example: 'x264 DTS' },
  { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]', footNotes: '1' },

  { token: '{MediaInfo AudioCodec}', example: 'DTS' },
  { token: '{MediaInfo AudioChannels}', example: '5.1' },
  {
    token: '{MediaInfo AudioLanguages}',
    example: '[EN+DE]',
    footNotes: '1,2',
  },
  {
    token: '{MediaInfo AudioLanguagesAll}',
    example: '[EN]',
    footNotes: '1',
  },
  { token: '{MediaInfo SubtitleLanguages}', example: '[DE]', footNotes: '1' },

  { token: '{MediaInfo VideoCodec}', example: 'x264' },
  { token: '{MediaInfo VideoBitDepth}', example: '10' },
  { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' },
  { token: '{MediaInfo VideoDynamicRangeType}', example: 'DV HDR10' },
];

const otherTokens = [
  { token: '{Release Group}', example: 'Rls Grp', footNotes: '1' },
  { token: '{Custom Formats}', example: 'iNTERNAL' },
  {
    token: '{Custom Format:FormatName}',
    example: 'Surround Sound',
    footNotes: '2',
  },
];

const otherAnimeTokens = [{ token: '{Release Hash}', example: 'ABCDEFGH' }];

const originalTokens = [
  {
    token: '{Original Title}',
    example: "The.Series.Title's!.S01E01.WEBDL.1080p.x264-EVOLVE",
  },
  {
    token: '{Original Filename}',
    example: "the.series.title's!.s01e01.webdl.1080p.x264-EVOLVE",
  },
];

function getNamingTokenGroups({
  season,
  episode,
  daily,
  anime,
  additional,
}: TokenFlags): TokenGroup[] {
  const groups: TokenGroup[] = [];

  if (episode) {
    const presets: Token[] = [];

    if (daily) {
      presets.push(...fileNameDailyTokens);
    }

    if (anime) {
      presets.push(...fileNameAnimeTokens);
    }

    presets.push(...fileNameTokens);

    groups.push({
      legend: translate('Presets'),
      caption: translate('NamingPresetsHelpText'),
      tokens: presets,
      size: sizes.LARGE,
      isFullFilename: true,
    });
  }

  groups.push({
    legend: translate('Series'),
    tokens: withFootNotes(seriesTokens, { 1: translate('SeriesFootNote') }),
  });

  groups.push({ legend: translate('SeriesID'), tokens: seriesIdTokens });

  if (season) {
    groups.push({ legend: translate('Season'), tokens: seasonTokens });
  }

  if (episode) {
    groups.push({ legend: translate('Episode'), tokens: episodeTokens });
    groups.push({ legend: translate('AirDate'), tokens: airDateTokens });

    if (anime) {
      groups.push({
        legend: translate('AbsoluteEpisodeNumber'),
        tokens: absoluteTokens,
      });
    }
  }

  if (additional) {
    groups.push({
      legend: translate('EpisodeTitle'),
      tokens: withFootNotes(episodeTitleTokens, {
        1: translate('EpisodeTitleFootNote'),
      }),
    });

    groups.push({ legend: translate('Quality'), tokens: qualityTokens });

    groups.push({
      legend: translate('MediaInfo'),
      tokens: withFootNotes(mediaInfoTokens, {
        1: translate('MediaInfoFootNote'),
        2: translate('MediaInfoFootNote2'),
      }),
    });

    groups.push({
      legend: translate('Other'),
      tokens: withFootNotes(
        anime ? [...otherTokens, ...otherAnimeTokens] : otherTokens,
        {
          1: translate('ReleaseGroupFootNote'),
          2: translate('CustomFormatFootNote'),
        }
      ),
    });

    groups.push({
      legend: translate('Original'),
      tokens: originalTokens,
      size: sizes.LARGE,
    });
  }

  return groups;
}

interface NamingModalProps {
  isOpen: boolean;
  name: keyof Pick<
    NamingSettingsModel,
    | 'standardEpisodeFormat'
    | 'dailyEpisodeFormat'
    | 'animeEpisodeFormat'
    | 'seriesFolderFormat'
    | 'seasonFolderFormat'
    | 'specialsFolderFormat'
  >;
  value: string;
  season?: boolean;
  episode?: boolean;
  daily?: boolean;
  anime?: boolean;
  additional?: boolean;
  onInputChange: ({ name, value }: { name: string; value: string }) => void;
  onModalClose: () => void;
}

function NamingModal(props: NamingModalProps) {
  const {
    isOpen,
    name,
    value,
    season = false,
    episode = false,
    daily = false,
    anime = false,
    additional = false,
    onInputChange,
    onModalClose,
  } = props;

  const [tokenSeparator, setTokenSeparator] = useState<TokenSeparator>(' ');
  const [tokenCase, setTokenCase] = useState<TokenCase>('title');
  const [selectionStart, setSelectionStart] = useState<number | null>(null);
  const [selectionEnd, setSelectionEnd] = useState<number | null>(null);

  const handleTokenSeparatorChange = useCallback(
    ({ value }: { value: TokenSeparator }) => {
      setTokenSeparator(value);
    },
    [setTokenSeparator]
  );

  const handleTokenCaseChange = useCallback(
    ({ value }: { value: TokenCase }) => {
      setTokenCase(value);
    },
    [setTokenCase]
  );

  const handleInputSelectionChange = useCallback(
    (selectionStart: number | null, selectionEnd: number | null) => {
      setSelectionStart(selectionStart);
      setSelectionEnd(selectionEnd);
    },
    [setSelectionStart, setSelectionEnd]
  );

  const handleOptionPress = useCallback(
    ({
      isFullFilename,
      tokenValue,
    }: {
      isFullFilename: boolean;
      tokenValue: string;
    }) => {
      if (isFullFilename) {
        onInputChange({ name, value: tokenValue });
      } else if (selectionStart == null || selectionEnd == null) {
        onInputChange({
          name,
          value: `${value}${tokenValue}`,
        });
      } else {
        const start = value.substring(0, selectionStart);
        const end = value.substring(selectionEnd);
        const newValue = `${start}${tokenValue}${end}`;
        const caret = selectionStart + tokenValue.length;

        onInputChange({ name, value: newValue });

        setSelectionStart(caret);
        setSelectionEnd(caret);
      }
    },
    [name, value, selectionEnd, selectionStart, onInputChange]
  );

  const tokenGroups = getNamingTokenGroups({
    season,
    episode,
    daily,
    anime,
    additional,
  });

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {episode
            ? translate('FileNameTokens')
            : translate('FolderNameTokens')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.builder}>
            <TextInput
              className={styles.formatInput}
              name={name}
              value={value}
              onChange={onInputChange}
              onSelectionChange={handleInputSelectionChange}
            />

            <div className={styles.controls}>
              <div className={styles.control}>
                <span className={styles.controlLabel}>
                  {translate('Separator')}
                </span>

                <SelectInput
                  className={styles.namingSelect}
                  name="separator"
                  value={tokenSeparator}
                  values={separatorOptions}
                  onChange={handleTokenSeparatorChange}
                />
              </div>

              <div className={styles.control}>
                <span className={styles.controlLabel}>
                  {translate('Capitalization')}
                </span>

                <SelectInput
                  className={styles.namingSelect}
                  name="case"
                  value={tokenCase}
                  values={caseOptions}
                  onChange={handleTokenCaseChange}
                />
              </div>
            </div>
          </div>

          <div className={styles.instruction}>
            <Icon name={icons.INFO} size={14} />
            <span>{translate('NamingTokenSelectHelpText')}</span>
          </div>

          {tokenGroups.map((group) => (
            <ModalSection key={group.legend} title={group.legend}>
              {group.caption ? (
                <p className={styles.caption}>{group.caption}</p>
              ) : null}

              <div className={styles.groups}>
                {group.tokens.map(({ token, example, notes }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    notes={notes}
                    isFullFilename={group.isFullFilename}
                    size={group.size}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </ModalSection>
          ))}
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default NamingModal;
