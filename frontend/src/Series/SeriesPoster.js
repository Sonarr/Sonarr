import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LazyLoad from 'react-lazyload';

const posterPlaceholder = 'data:image/png;base64,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';

function findPoster(images) {
  return _.find(images, { coverType: 'poster' });
}

function getPosterUrl(poster, size) {
  if (poster) {
    // Remove protocol
    let url = poster.url.replace(/^https?:/, '');
    url = url.replace('poster.jpg', `poster-${size}.jpg`);

    return url;
  }
}

class SeriesPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const pixelRatio = Math.floor(window.devicePixelRatio);

    const {
      images,
      size
    } = props;

    const poster = findPoster(images);

    this.state = {
      pixelRatio,
      poster,
      posterUrl: getPosterUrl(poster, pixelRatio * size),
      isLoaded: false,
      hasError: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      images,
      size
    } = this.props;

    const {
      poster,
      pixelRatio
    } = this.state;

    const nextPoster = findPoster(images);

    if (nextPoster && (!poster || nextPoster.url !== poster.url)) {
      this.setState({
        poster: nextPoster,
        posterUrl: getPosterUrl(nextPoster, pixelRatio * size),
        hasError: false
        // Don't reset isLoaded, as we want to immediately try to
        // show the new image, whether an image was shown previously
        // or the placeholder was shown.
      });
    }
  }

  //
  // Listeners

  onError = () => {
    this.setState({ hasError: true });
  }

  onLoad = () => {
    this.setState({
      isLoaded: true,
      hasError: false
    });
  }

  //
  // Render

  render() {
    const {
      className,
      style,
      size,
      lazy,
      overflow
    } = this.props;

    const {
      posterUrl,
      hasError,
      isLoaded
    } = this.state;

    if (hasError || !posterUrl) {
      return (
        <img
          className={className}
          style={style}
          src={posterPlaceholder}
        />
      );
    }

    if (lazy) {
      return (
        <LazyLoad
          height={size}
          offset={100}
          overflow={overflow}
          placeholder={
            <img
              className={className}
              style={style}
              src={posterPlaceholder}
            />
          }
        >
          <img
            className={className}
            style={style}
            src={posterUrl}
            onError={this.onError}
          />
        </LazyLoad>
      );
    }

    return (
      <img
        className={className}
        style={style}
        src={isLoaded ? posterUrl : posterPlaceholder}
        onError={this.onError}
        onLoad={this.onLoad}
      />
    );
  }
}

SeriesPoster.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool.isRequired,
  overflow: PropTypes.bool.isRequired
};

SeriesPoster.defaultProps = {
  size: 250,
  lazy: true,
  overflow: false
};

export default SeriesPoster;
