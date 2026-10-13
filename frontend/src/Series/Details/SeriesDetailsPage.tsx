import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSeries from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import SeriesDetails from './SeriesDetails';

function SeriesDetailsPage() {
  const { data: allSeries } = useSeries();
  const { titleSlug } = useParams<{ titleSlug: string }>();
  const navigate = useNavigate();

  const series = allSeries.find((series) => series.titleSlug === titleSlug);
  const seriesId = series?.id;
  const previousSeriesId = usePrevious(seriesId);
  const previousTitleSlug = usePrevious(titleSlug);

  useEffect(() => {
    if (
      seriesId !== undefined ||
      previousSeriesId === undefined ||
      titleSlug !== previousTitleSlug
    ) {
      return;
    }

    const renamedSeries = allSeries.find(
      (series) => series.id === previousSeriesId
    );

    if (renamedSeries) {
      navigate(`/series/${renamedSeries.titleSlug}`, { replace: true });
    } else {
      navigate('/');
    }
  }, [
    allSeries,
    seriesId,
    previousSeriesId,
    titleSlug,
    previousTitleSlug,
    navigate,
  ]);

  if (!series) {
    return <NotFound message={translate('SeriesCannotBeFound')} />;
  }

  return <SeriesDetails seriesId={series.id} />;
}

export default SeriesDetailsPage;
