import React, { useCallback, useState } from 'react';
import FieldSet from 'Components/FieldSet';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, sizes } from 'Helpers/Props';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingOption from './NamingOption';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import styles from './NamingModal.css';

const separatorOptions: { key: TokenSeparator; value: string }[] = [
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

const caseOptions: { key: TokenCase; value: string }[] = [
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
  { token: '{Series Title}', example: "The Series Title's!", footNote: true },
  {
    token: '{Series CleanTitle}',
    example: "The Series Title's!",
    footNote: true,
  },
  {
    token: '{Series TitleYear}',
    example: "The Series Title's! (2010)",
    footNote: true,
  },
  {
    token: '{Series CleanTitleYear}',
    example: "The Series Title's! 2010",
    footNote: true,
  },
  {
    token: '{Series TitleWithoutYear}',
    example: "The Series Title's!",
    footNote: true,
  },
  {
    token: '{Series CleanTitleWithoutYear}',
    example: "The Series Title's!",
    footNote: true,
  },
  {
    token: '{Series TitleThe}',
    example: "Series Title's!, The",
    footNote: true,
  },
  {
    token: '{Series CleanTitleThe}',
    example: "Series Title's!, The",
    footNote: true,
  },
  {
    token: '{Series TitleTheYear}',
    example: "Series Title's!, The (2010)",
    footNote: true,
  },
  {
    token: '{Series CleanTitleTheYear}',
    example: "Series Title's!, The 2010",
    footNote: true,
  },
  {
    token: '{Series TitleTheWithoutYear}',
    example: "Series Title's!, The",
    footNote: true,
  },
  {
    token: '{Series CleanTitleTheWithoutYear}',
    example: "Series Title's!, The",
    footNote: true,
  },
  { token: '{Series TitleFirstCharacter}', example: 'S', footNote: true },
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
  { token: '{Episode Title}', example: "Episode's Title", footNote: true },
  { token: '{Episode CleanTitle}', example: 'Episodes Title', footNote: true },
];

const qualityTokens = [
  { token: '{Quality Full}', example: 'WEBDL-1080p Proper' },
  { token: '{Quality Title}', example: 'WEBDL-1080p' },
];

const mediaInfoTokens = [
  { token: '{MediaInfo Simple}', example: 'x264 DTS' },
  { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]', footNote: true },

  { token: '{MediaInfo AudioCodec}', example: 'DTS' },
  { token: '{MediaInfo AudioChannels}', example: '5.1' },
  { token: '{MediaInfo AudioLanguages}', example: '[EN+DE]', footNote: true },
  { token: '{MediaInfo SubtitleLanguages}', example: '[DE]', footNote: true },

  { token: '{MediaInfo VideoCodec}', example: 'x264' },
  { token: '{MediaInfo VideoBitDepth}', example: '10' },
  { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' },
  { token: '{MediaInfo VideoDynamicRangeType}', example: 'DV HDR10' },
];

const otherTokens = [
  { token: '{Release Group}', example: 'Rls Grp', footNote: true },
  { token: '{Custom Formats}', example: 'iNTERNAL' },
  { token: '{Custom Format:FormatName}', example: 'AMZN' },
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

interface NamingModalProps {
  isOpen: boolean;
  name: keyof Pick<
    NamingConfig,
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
    (selectionStart: number, selectionEnd: number) => {
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

        onInputChange({ name, value: newValue });

        setSelectionStart(newValue.length - 1);
        setSelectionEnd(newValue.length - 1);
      }
    },
    [name, value, selectionEnd, selectionStart, onInputChange]
  );

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {episode
            ? translate('FileNameTokens')
            : translate('FolderNameTokens')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.namingSelectContainer}>
            <SelectInput
              className={styles.namingSelect}
              name="separator"
              value={tokenSeparator}
              values={separatorOptions}
              onChange={handleTokenSeparatorChange}
            />

            <SelectInput
              className={styles.namingSelect}
              name="case"
              value={tokenCase}
              values={caseOptions}
              onChange={handleTokenCaseChange}
            />
          </div>

          {episode ? (
            <FieldSet legend={translate('FileNames')}>
              <div className={styles.groups}>
                {daily
                  ? fileNameDailyTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        isFullFilename={true}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        size={sizes.LARGE}
                        onPress={handleOptionPress}
                      />
                    ))
                  : null}

                {anime
                  ? fileNameAnimeTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        isFullFilename={true}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        size={sizes.LARGE}
                        onPress={handleOptionPress}
                      />
                    ))
                  : null}

                {fileNameTokens.map(({ token, example }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    isFullFilename={true}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    size={sizes.LARGE}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </FieldSet>
          ) : null}

          <FieldSet legend={translate('Series')}>
            <div className={styles.groups}>
              {seriesTokens.map(({ token, example, footNote }) => (
                <NamingOption
                  key={token}
                  token={token}
                  example={example}
                  footNote={footNote}
                  tokenSeparator={tokenSeparator}
                  tokenCase={tokenCase}
                  onPress={handleOptionPress}
                />
              ))}
            </div>

            <div className={styles.footNote}>
              <Icon className={styles.icon} name={icons.FOOTNOTE} />
              <InlineMarkdown data={translate('SeriesFootNote')} />
            </div>
          </FieldSet>

          <FieldSet legend={translate('SeriesID')}>
            <div className={styles.groups}>
              {seriesIdTokens.map(({ token, example }) => (
                <NamingOption
                  key={token}
                  token={token}
                  example={example}
                  tokenSeparator={tokenSeparator}
                  tokenCase={tokenCase}
                  onPress={handleOptionPress}
                />
              ))}
            </div>
          </FieldSet>

          {season ? (
            <FieldSet legend={translate('Season')}>
              <div className={styles.groups}>
                {seasonTokens.map(({ token, example }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </FieldSet>
          ) : null}

          {episode ? (
            <div>
              <FieldSet legend={translate('Episode')}>
                <div className={styles.groups}>
                  {episodeTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              <FieldSet legend={translate('AirDate')}>
                <div className={styles.groups}>
                  {airDateTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              {anime ? (
                <FieldSet legend={translate('AbsoluteEpisodeNumber')}>
                  <div className={styles.groups}>
                    {absoluteTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    ))}
                  </div>
                </FieldSet>
              ) : null}
            </div>
          ) : null}

          {additional ? (
            <div>
              <FieldSet legend={translate('EpisodeTitle')}>
                <div className={styles.groups}>
                  {episodeTitleTokens.map(({ token, example, footNote }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNote={footNote}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('EpisodeTitleFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Quality')}>
                <div className={styles.groups}>
                  {qualityTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              <FieldSet legend={translate('MediaInfo')}>
                <div className={styles.groups}>
                  {mediaInfoTokens.map(({ token, example, footNote }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNote={footNote}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>

                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('MediaInfoFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Other')}>
                <div className={styles.groups}>
                  {otherTokens.map(({ token, example, footNote }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNote={footNote}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}

                  {anime
                    ? otherAnimeTokens.map(({ token, example }) => (
                        <NamingOption
                          key={token}
                          token={token}
                          example={example}
                          tokenSeparator={tokenSeparator}
                          tokenCase={tokenCase}
                          onPress={handleOptionPress}
                        />
                      ))
                    : null}
                </div>

                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('ReleaseGroupFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Original')}>
                <div className={styles.groups}>
                  {originalTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      size={sizes.LARGE}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>
            </div>
          ) : null}
        </ModalBody>

        <ModalFooter>
          <TextInput
            name={name}
            value={value}
            onChange={onInputChange}
            onSelectionChange={handleInputSelectionChange}
          />

          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default NamingModal;
