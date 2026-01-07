import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import LazyLoad from 'react-lazyload';
import translate from 'Utilities/String/translate';
import { CoverType, Image } from './Series';

function findImage(images: Image[], coverType: CoverType) {
  return images.find((image) => image.coverType === coverType);
}

function getUrl(image: Image, coverType: CoverType, size: number) {
  const imageUrl = image?.url;

  return imageUrl
    ? imageUrl.replace(`${coverType}.jpg`, `${coverType}-${size}.jpg`)
    : null;
}

export interface SeriesImageProps {
  className?: string;
  style?: object;
  images: Image[];
  coverType: CoverType;
  placeholder: string;
  size?: number;
  lazy?: boolean;
  overflow?: boolean;
  title: string;
  onError?: () => void;
  onLoad?: () => void;
}

const pixelRatio = Math.max(Math.round(window.devicePixelRatio), 1);

function SeriesImage({
  className,
  style,
  images,
  coverType,
  placeholder,
  size = 250,
  lazy = true,
  overflow = false,
  title,
  onError,
  onLoad,
}: SeriesImageProps) {
  const [url, setUrl] = useState<string | null>(null);
  const [hasError, setHasError] = useState(false);
  const [isLoaded, setIsLoaded] = useState(true);
  const image = useRef<Image | null>(null);

  const alt = useMemo(() => {
    let type = translate('ImagePoster');

    switch (coverType) {
      case 'banner':
        type = translate('Banner');
        break;
      case 'fanart':
        type = translate('ImageFanart');
        break;
      case 'season':
        type = translate('ImageSeason');
        break;
      default:
        break;
    }

    return `${title} ${type}`;
  }, [title, coverType]);

  const handleLoad = useCallback(() => {
    setHasError(false);
    setIsLoaded(true);
    onLoad?.();
  }, [setHasError, setIsLoaded, onLoad]);

  const handleError = useCallback(() => {
    setHasError(true);
    setIsLoaded(false);
    onError?.();
  }, [setHasError, setIsLoaded, onError]);

  useEffect(() => {
    const nextImage = findImage(images, coverType);

    if (nextImage && (!image.current || nextImage.url !== image.current.url)) {
      // Don't reset isLoaded, as we want to immediately try to
      // show the new image, whether an image was shown previously
      // or the placeholder was shown.
      image.current = nextImage;

      setUrl(getUrl(nextImage, coverType, pixelRatio * size));
      setHasError(false);
    } else if (!nextImage) {
      if (image.current) {
        image.current = null;
        setUrl(placeholder);
        setHasError(false);
        onError?.();
      }
    }
  }, [images, coverType, placeholder, size, onError]);

  useEffect(() => {
    if (!image.current) {
      onError?.();
    }
    // This should only run once when the component mounts,
    // so we don't need to include the other dependencies.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (hasError || !url) {
    return <img className={className} style={style} src={placeholder} />;
  }

  if (lazy) {
    return (
      <LazyLoad
        height={size}
        offset={100}
        overflow={overflow}
        placeholder={
          <img className={className} style={style} src={placeholder} />
        }
      >
        <img
          alt={alt}
          className={className}
          style={style}
          src={url}
          rel="noreferrer"
          onError={handleError}
          onLoad={handleLoad}
        />
      </LazyLoad>
    );
  }

  return (
    <img
      alt={alt}
      className={className}
      style={style}
      src={isLoaded ? url : placeholder}
      onError={handleError}
      onLoad={handleLoad}
    />
  );
}

export default SeriesImage;
