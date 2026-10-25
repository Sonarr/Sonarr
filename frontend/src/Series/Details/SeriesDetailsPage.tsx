import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSeries from 'Series/useSeries';
import getSeriesPath from 'Utilities/Series/getSeriesPath';
import translate from 'Utilities/String/translate';
import SeriesDetails from './SeriesDetails';

function SeriesDetailsPage() {
  const { data: allSeries } = useSeries();
  const { seriesId: seriesIdParam, titleSlug } = useParams<{
    seriesId?: string;
    titleSlug: string;
  }>();
  const navigate = useNavigate();

  const series = seriesIdParam
    ? allSeries.find((series) => series.id === Number(seriesIdParam))
    : allSeries.find((series) => series.titleSlug === titleSlug);

  const seriesId = series?.id;
  const previousSeriesId = usePrevious(seriesId);
  const previousSeriesIdParam = usePrevious(seriesIdParam);

  useEffect(() => {
    if (series) {
      if (!seriesIdParam || series.titleSlug !== titleSlug) {
        navigate(getSeriesPath(series), { replace: true });
      }

      return;
    }

    if (
      previousSeriesId !== undefined &&
      seriesIdParam === previousSeriesIdParam
    ) {
      navigate('/');
    }
  }, [
    series,
    seriesIdParam,
    titleSlug,
    previousSeriesId,
    previousSeriesIdParam,
    navigate,
  ]);

  if (!series) {
    return <NotFound message={translate('SeriesCannotBeFound')} />;
  }

  return <SeriesDetails seriesId={series.id} />;
}

export default SeriesDetailsPage;
