import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LazyLoad from 'react-lazyload';

function findImage(images, coverType) {
  return images.find((image) => image.coverType === coverType);
}

function getUrl(image, coverType, size) {
  if (image) {
    // Remove protocol
    let url = image.url.replace(/^https?:/, '');
    url = url.replace(`${coverType}.jpg`, `${coverType}-${size}.jpg`);

    return url;
  }
}

class SeriesImage extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const pixelRatio = Math.max(Math.round(window.devicePixelRatio), 1);

    const {
      images,
      coverType,
      size
    } = props;

    const image = findImage(images, coverType);

    this.state = {
      pixelRatio,
      image,
      url: getUrl(image, coverType, pixelRatio * size),
      isLoaded: false,
      hasError: false
    };
  }

  componentDidMount() {
    if (!this.state.url && this.props.onError) {
      this.props.onError();
    }
  }

  componentDidUpdate() {
    const {
      images,
      coverType,
      placeholder,
      size,
      onError
    } = this.props;

    const {
      image,
      pixelRatio
    } = this.state;

    const nextImage = findImage(images, coverType);

    if (nextImage && (!image || nextImage.url !== image.url)) {
      this.setState({
        image: nextImage,
        url: getUrl(nextImage, coverType, pixelRatio * size),
        hasError: false
        // Don't reset isLoaded, as we want to immediately try to
        // show the new image, whether an image was shown previously
        // or the placeholder was shown.
      });
    } else if (!nextImage && image) {
      this.setState({
        image: nextImage,
        url: placeholder,
        hasError: false
      });

      if (onError) {
        onError();
      }
    }
  }

  //
  // Listeners

  onError = () => {
    this.setState({
      hasError: true
    });

    if (this.props.onError) {
      this.props.onError();
    }
  };

  onLoad = () => {
    this.setState({
      isLoaded: true,
      hasError: false
    });

    if (this.props.onLoad) {
      this.props.onLoad();
    }
  };

  //
  // Render

  render() {
    const {
      className,
      style,
      placeholder,
      size,
      lazy,
      overflow
    } = this.props;

    const {
      url,
      hasError,
      isLoaded
    } = this.state;

    if (hasError || !url) {
      return (
        <img
          className={className}
          style={style}
          src={placeholder}
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
              src={placeholder}
            />
          }
        >
          <img
            className={className}
            style={style}
            src={url}
            onError={this.onError}
            onLoad={this.onLoad}
            rel="noreferrer"
          />
        </LazyLoad>
      );
    }

    return (
      <img
        className={className}
        style={style}
        src={isLoaded ? url : placeholder}
        onError={this.onError}
        onLoad={this.onLoad}
      />
    );
  }
}

SeriesImage.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  coverType: PropTypes.string.isRequired,
  placeholder: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool.isRequired,
  overflow: PropTypes.bool.isRequired,
  onError: PropTypes.func,
  onLoad: PropTypes.func
};

SeriesImage.defaultProps = {
  size: 250,
  lazy: true,
  overflow: false
};

export default SeriesImage;
