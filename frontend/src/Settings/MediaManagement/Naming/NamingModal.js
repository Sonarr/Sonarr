import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import NamingOption from './NamingOption';
import styles from './NamingModal.css';

class NamingModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      case: 'title'
    };
  }

  //
  // Listeners

  onNamingCaseChange = (event) => {
    this.setState({ case: event.value });
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
      daily,
      anime,
      additional,
      onInputChange,
      onModalClose
    } = this.props;

    const namingOptions = [
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
      { token: '{Series Title}', example: 'Series Title (2010)' },
      { token: '{Series.Title}', example: 'Series.Title.(2010)' },
      { token: '{Series_Title}', example: 'Series_Title_(2010)' },

      { token: '{Series TitleThe}', example: 'Series Title, The (2010)' },

      { token: '{Series CleanTitle}', example: 'Series Title 2010' },
      { token: '{Series.CleanTitle}', example: 'Series.Title.2010' },
      { token: '{Series_CleanTitle}', example: 'Series_Title_2010' }
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
      { token: '{Air Date}', example: '2016 03 20' },
      { token: '{Air.Date}', example: '2016.03.20' },
      { token: '{Air_Date}', example: '2016_03_20' }
    ];

    const absoluteTokens = [
      { token: '{absolute:0}', example: '1' },
      { token: '{absolute:00}', example: '01' },
      { token: '{absolute:000}', example: '001' }
    ];

    const episodeTitleTokens = [
      { token: '{Episode Title}', example: 'Episode Title' },
      { token: '{Episode.Title}', example: 'Episode.Title' },
      { token: '{Episode_Title}', example: 'Episode_Title' },
      { token: '{Episode CleanTitle}', example: 'Episode Title' },
      { token: '{Episode.CleanTitle}', example: 'Episode.Title' },
      { token: '{Episode_CleanTitle}', example: 'Episode_Title' }
    ];

    const qualityTokens = [
      { token: '{Quality Full}', example: 'HDTV 720p Proper' },
      { token: '{Quality-Full}', example: 'HDTV-720p-Proper' },
      { token: '{Quality.Full}', example: 'HDTV.720p.Proper' },
      { token: '{Quality_Full}', example: 'HDTV_720p_Proper' },
      { token: '{Quality Title}', example: 'HDTV 720p' },
      { token: '{Quality-Title}', example: 'HDTV-720p' },
      { token: '{Quality.Title}', example: 'HDTV.720p' },
      { token: '{Quality_Title}', example: 'HDTV_720p' }
    ];

    const mediaInfoTokens = [
      { token: '{MediaInfo Simple}', example: 'x264 DTS' },
      { token: '{MediaInfo.Simple}', example: 'x264.DTS' },
      { token: '{MediaInfo_Simple}', example: 'x264_DTS' },
      { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]' },
      { token: '{MediaInfo.Full}', example: 'x264.DTS.[EN+DE]' },
      { token: '{MediaInfo_Full}', example: 'x264_DTS_[EN+DE]' },
      { token: '{MediaInfo VideoCodec}', example: 'x264' },
      { token: '{MediaInfo AudioFormat}', example: 'DTS' },
      { token: '{MediaInfo AudioChannels}', example: '5.1' }
    ];

    const releaseGroupTokens = [
      { token: '{Release Group}', example: 'Rls Grp' },
      { token: '{Release.Group}', example: 'Rls.Grp' },
      { token: '{Release_Group}', example: 'Rls_Grp' }
    ];

    const originalTokens = [
      { token: '{Original Title}', example: 'Series.Title.S01E01.HDTV.x264-EVOLVE' },
      { token: '{Original Filename}', example: 'series.title.s01e01.hdtv.x264-EVOLVE' }
    ];

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
                name="namingSelect"
                value={this.state.case}
                values={namingOptions}
                onChange={this.onNamingCaseChange}
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
                            tokenCase={this.state.case}
                            size={sizes.LARGE}
                            onInputChange={onInputChange}
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
                        tokenCase={this.state.case}
                        onInputChange={onInputChange}
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
                            tokenCase={this.state.case}
                            onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  {
                    daily &&
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
                                  tokenCase={this.state.case}
                                  onInputChange={onInputChange}
                                />
                              );
                            }
                            )
                          }
                        </div>
                      </FieldSet>
                  }

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
                                  tokenCase={this.state.case}
                                  onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                        mediaInfoTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Release Group">
                    <div className={styles.groups}>
                      {
                        releaseGroupTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              size={sizes.LARGE}
                              onInputChange={onInputChange}
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
