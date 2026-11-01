import { throttle } from 'lodash';
import { queryClient } from 'App/queryClient';

let isRefreshBlocked = false;
let hasPendingRefresh = false;

export function refreshQueue() {
  if (isRefreshBlocked) {
    hasPendingRefresh = true;
    return;
  }

  queryClient.invalidateQueries({ queryKey: ['/queue'] });
}

export function forceRefreshQueue() {
  hasPendingRefresh = false;
  queryClient.invalidateQueries({ queryKey: ['/queue'] });
}

export const throttledRefreshQueue = throttle(refreshQueue, 5000);

export function setQueueRefreshBlocked(isBlocked: boolean) {
  isRefreshBlocked = isBlocked;

  if (!isBlocked && hasPendingRefresh) {
    hasPendingRefresh = false;
    refreshQueue();
  }
}
