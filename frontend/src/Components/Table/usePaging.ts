import { useCallback, useMemo } from 'react';
import { useDispatch } from 'react-redux';

interface PagingOptions {
  page: number;
  totalPages: number;
  gotoPage: ({ page }: { page: number }) => void;
}

function usePaging(options: PagingOptions) {
  const { page, totalPages, gotoPage } = options;
  const dispatch = useDispatch();

  const handleFirstPagePress = useCallback(() => {
    dispatch(gotoPage({ page: 1 }));
  }, [dispatch, gotoPage]);

  const handlePreviousPagePress = useCallback(() => {
    dispatch(gotoPage({ page: Math.max(page - 1, 1) }));
  }, [page, dispatch, gotoPage]);

  const handleNextPagePress = useCallback(() => {
    dispatch(gotoPage({ page: Math.min(page + 1, totalPages) }));
  }, [page, totalPages, dispatch, gotoPage]);

  const handleLastPagePress = useCallback(() => {
    dispatch(gotoPage({ page: totalPages }));
  }, [totalPages, dispatch, gotoPage]);

  const handlePageSelect = useCallback(
    (page: number) => {
      dispatch(gotoPage({ page }));
    },
    [dispatch, gotoPage]
  );

  return useMemo(() => {
    return {
      handleFirstPagePress,
      handlePreviousPagePress,
      handleNextPagePress,
      handleLastPagePress,
      handlePageSelect,
    };
  }, [
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  ]);
}

export default usePaging;
