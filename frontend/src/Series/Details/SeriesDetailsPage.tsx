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

  const seriesIndex = allSeries.findIndex(
    (series) => series.titleSlug === titleSlug
  );

  const previousIndex = usePrevious(seriesIndex);

  useEffect(() => {
    if (
      seriesIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      navigate('/');
    }
  }, [seriesIndex, previousIndex, navigate]);

  if (seriesIndex === -1) {
    return <NotFound message={translate('SeriesCannotBeFound')} />;
  }

  return <SeriesDetails seriesId={allSeries[seriesIndex].id} />;
}

export default SeriesDetailsPage;
