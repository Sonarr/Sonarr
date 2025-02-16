const anySignal = (
  ...signals: (AbortSignal | null | undefined)[]
): AbortSignal => {
  const controller = new AbortController();

  for (const signal of signals.filter(Boolean) as AbortSignal[]) {
    if (signal.aborted) {
      // Break early if one of the signals is already aborted.
      controller.abort();

      break;
    }

    // Listen for abort events on the provided signals and abort the controller.
    // Automatically removes listeners when the controller is aborted.
    signal.addEventListener('abort', () => controller.abort(signal.reason), {
      signal: controller.signal,
    });
  }

  return controller.signal;
};

export default anySignal;
