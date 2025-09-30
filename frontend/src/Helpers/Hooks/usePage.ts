import { useEffect } from 'react';
import { useHistory } from 'react-router';
import { create } from 'zustand';

interface PageStore {
  blocklist: number;
  events: number;
  queue: number;
}

const pageStore = create<PageStore>(() => ({
  blocklist: 1,
  events: 1,
  queue: 1,
}));

const usePage = (kind: keyof PageStore) => {
  const { action } = useHistory();

  const goToPage = (page: number) => {
    pageStore.setState({ [kind]: page });
  };

  useEffect(() => {
    if (action === 'POP') {
      pageStore.setState({ [kind]: 1 });
    }
  }, [action, kind]);

  return {
    page: pageStore((state) => state[kind]),
    goToPage,
  };
};

export default usePage;

export const resetPage = (kind: keyof PageStore) => {
  pageStore.setState({ [kind]: 1 });
};
