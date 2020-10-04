import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes, icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import NamingOption from './NamingOption';
import styles from './NamingModal.css';

const separatorOptions = [
  { key: ' ', value: 'Space ( )' },
  { key: '.', value: 'Period (.)' },
  { key: '_', value: 'Underscore (_)' },
  { key: '-', value: 'Dash (-)' }
];

const caseOptions = [
  { key: 'title', value: 'Default Case' },
  { key: 'lower', value: 'Lower Case' },
  { key: 'upper', value: 'Upper Case' }
];

const fileNameTokens = [
  {
    token: '{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}',
    example: 'Series Title (2010) - S01E01 - Episode Title HDTV-720p Proper'
  },
  {
    token: '{Series Title} - {season:0}x{episode:00} - {Episode Title} {Quality Full}',
    example: 'Series Title (2010) - 1x01 - Episode Title HDTV-720p Proper'
  },
  {
    token: '{Series.Title}.S{season:00}E{episode:00}.{EpisodeClean.Title}.{Quality.Full}',
    example: 'Series.Title.(2010).S01E01.Episode.Title.HDTV-720p'
  }
];

const seriesTokens = [
  { token: '{Series Title}', example: 'Series Title!' },
  { token: '{Series CleanTitle}', example: 'Series Title' },
  { token: '{Series CleanTitleYear}', example: 'Series Title 2010' },
  { token: '{Series TitleThe}', example: 'Series Title, The' },
  { token: '{Series TitleTheYear}', example: 'Series Title, The (2010)' },
  { token: '{Series TitleYear}', example: 'Series Title (2010)' },
  { token: '{Series TitleFirstCharacter}', example: 'S' }
];

const seriesIdTokens = [
  { token: '{ImdbId}', example: 'tt12345' },
  { token: '{TvdbId}', example: '12345' },
  { token: '{TvMazeId}', example: '54321' }
];

const seasonTokens = [
  { token: '{season:0}', example: '1' },
  { token: '{season:00}', example: '01' }
];

const episodeTokens = [
  { token: '{episode:0}', example: '1' },
  { token: '{episode:00}', example: '01' }
];

const airDateTokens = [
  { token: '{Air-Date}', example: '2016-03-20' },
  { token: '{Air Date}', example: '2016 03 20' }
];

const absoluteTokens = [
  { token: '{absolute:0}', example: '1' },
  { token: '{absolute:00}', example: '01' },
  { token: '{absolute:000}', example: '001' }
];

const episodeTitleTokens = [
  { token: '{Episode Title}', example: 'Episode Title' },
  { token: '{Episode CleanTitle}', example: 'Episode Title' }
];

const qualityTokens = [
  { token: '{Quality Full}', example: 'HDTV 720p Proper' },
  { token: '{Quality Title}', example: 'HDTV 720p' }
];

const mediaInfoTokens = [
  { token: '{MediaInfo Simple}', example: 'x264 DTS' },
  { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]', footNote: 1 },

  { token: '{MediaInfo AudioCodec}', example: 'DTS' },
  { token: '{MediaInfo AudioChannels}', example: '5.1' },
  { token: '{MediaInfo AudioLanguages}', example: '[EN+DE]', footNote: 1 },
  { token: '{MediaInfo SubtitleLanguages}', example: '[DE]', footNote: 1 },

  { token: '{MediaInfo VideoCodec}', example: 'x264' },
  { token: '{MediaInfo VideoBitDepth}', example: '10' },
  { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' }
];

const otherTokens = [
  { token: '{Release Group}', example: 'Rls Grp' },
  { token: '{Preferred Words}', example: 'iNTERNAL' }
];

const originalTokens = [
  { token: '{Original Title}', example: 'Series.Title.S01E01.HDTV.x264-EVOLVE' },
  { token: '{Original Filename}', example: 'series.title.s01e01.hdtv.x264-EVOLVE' }
];

class NamingModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._selectionStart = null;
    this._selectionEnd = null;

    this.state = {
      separator: ' ',
      case: 'title'
    };
  }

  //
  // Listeners

  onTokenSeparatorChange = (event) => {
    this.setState({ separator: event.value });
  }

  onTokenCaseChange = (event) => {
    this.setState({ case: event.value });
  }

  onInputSelectionChange = (selectionStart, selectionEnd) => {
    this._selectionStart = selectionStart;
    this._selectionEnd = selectionEnd;
  }

  onOptionPress = ({ isFullFilename, tokenValue }) => {
    const {
      name,
      value,
      onInputChange
    } = this.props;

    const selectionStart = this._selectionStart;
    const selectionEnd = this._selectionEnd;

    if (isFullFilename) {
      onInputChange({ name, value: tokenValue });
    } else if (selectionStart == null) {
      onInputChange({
        name,
        value: `${value}${tokenValue}`
      });
    } else {
      const start = value.substring(0, selectionStart);
      const end = value.substring(selectionEnd);
      const newValue = `${start}${tokenValue}${end}`;

      onInputChange({ name, value: newValue });
      this._selectionStart = newValue.length - 1;
      this._selectionEnd = newValue.length - 1;
    }
  }

  //
  // Render

  render() {
    const {
      name,
      value,
      isOpen,
      advancedSettings,
      season,
      episode,
      anime,
      additional,
      onInputChange,
      onModalClose
    } = this.props;

    const {
      separator: tokenSeparator,
      case: tokenCase
    } = this.state;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <ModalContent onModalClose={onModalClose}>
          <ModalHeader>
            File Name Tokens
          </ModalHeader>

          <ModalBody>
            <div className={styles.namingSelectContainer}>
              <SelectInput
                className={styles.namingSelect}
                name="separator"
                value={tokenSeparator}
                values={separatorOptions}
                onChange={this.onTokenSeparatorChange}
              />

              <SelectInput
                className={styles.namingSelect}
                name="case"
                value={tokenCase}
                values={caseOptions}
                onChange={this.onTokenCaseChange}
              />
            </div>

            {
              !advancedSettings &&
                <FieldSet legend="File Names">
                  <div className={styles.groups}>
                    {
                      fileNameTokens.map(({ token, example }) => {
                        return (
                          <NamingOption
                            key={token}
                            name={name}
                            value={value}
                            token={token}
                            example={example}
                            isFullFilename={true}
                            tokenSeparator={tokenSeparator}
                            tokenCase={tokenCase}
                            size={sizes.LARGE}
                            onPress={this.onOptionPress}
                          />
                        );
                      }
                      )
                    }
                  </div>
                </FieldSet>
            }

            <FieldSet legend="Series">
              <div className={styles.groups}>
                {
                  seriesTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        name={name}
                        value={value}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={this.onOptionPress}
                      />
                    );
                  }
                  )
                }
              </div>
            </FieldSet>

            <FieldSet legend="Series ID">
              <div className={styles.groups}>
                {
                  seriesIdTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        name={name}
                        value={value}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={this.onOptionPress}
                      />
                    );
                  }
                  )
                }
              </div>
            </FieldSet>

            {
              season &&
                <FieldSet legend="Season">
                  <div className={styles.groups}>
                    {
                      seasonTokens.map(({ token, example }) => {
                        return (
                          <NamingOption
                            key={token}
                            name={name}
                            value={value}
                            token={token}
                            example={example}
                            tokenSeparator={tokenSeparator}
                            tokenCase={tokenCase}
                            onPress={this.onOptionPress}
                          />
                        );
                      }
                      )
                    }
                  </div>
                </FieldSet>
            }

            {
              episode &&
                <div>
                  <FieldSet legend="Episode">
                    <div className={styles.groups}>
                      {
                        episodeTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Air-Date">
                    <div className={styles.groups}>
                      {
                        airDateTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  {
                    anime &&
                      <FieldSet legend="Absolute Episode Number">
                        <div className={styles.groups}>
                          {
                            absoluteTokens.map(({ token, example }) => {
                              return (
                                <NamingOption
                                  key={token}
                                  name={name}
                                  value={value}
                                  token={token}
                                  example={example}
                                  tokenSeparator={tokenSeparator}
                                  tokenCase={tokenCase}
                                  onPress={this.onOptionPress}
                                />
                              );
                            }
                            )
                          }
                        </div>
                      </FieldSet>
                  }
                </div>
            }

            {
              additional &&
                <div>
                  <FieldSet legend="Episode Title">
                    <div className={styles.groups}>
                      {
                        episodeTitleTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Quality">
                    <div className={styles.groups}>
                      {
                        qualityTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Media Info">
                    <div className={styles.groups}>
                      {
                        mediaInfoTokens.map(({ token, example, footNote }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              footNote={footNote}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>

                    <div className={styles.footNote}>
                      <Icon className={styles.icon} name={icons.FOOTNOTE} />
                      <div>
                        MediaInfo Full/AudioLanguages/SubtitleLanguages support a <code>:EN+DE</code> suffix allowing you to filter the languages included in the filename. Use <code>-DE</code> to exclude specific languages.
                        Appending <code>+</code> (eg <code>:EN+</code>) will output <code>[EN]</code>/<code>[EN+--]</code>/<code>[--]</code> depending on excluded languages. For example <code>{'{'}MediaInfo Full:EN+DE{'}'}</code>.
                      </div>
                    </div>
                  </FieldSet>

                  <FieldSet legend="Other">
                    <div className={styles.groups}>
                      {
                        otherTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Original">
                    <div className={styles.groups}>
                      {
                        originalTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              size={sizes.LARGE}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>
                </div>
            }
          </ModalBody>

          <ModalFooter>
            <TextInput
              name={name}
              value={value}
              onChange={onInputChange}
              onSelectionChange={this.onInputSelectionChange}
            />
            <Button onPress={onModalClose}>
              Close
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

NamingModal.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  isOpen: PropTypes.bool.isRequired,
  advancedSettings: PropTypes.bool.isRequired,
  season: PropTypes.bool.isRequired,
  episode: PropTypes.bool.isRequired,
  daily: PropTypes.bool.isRequired,
  anime: PropTypes.bool.isRequired,
  additional: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

NamingModal.defaultProps = {
  season: false,
  episode: false,
  daily: false,
  anime: false,
  additional: false
};

export default NamingModal;
