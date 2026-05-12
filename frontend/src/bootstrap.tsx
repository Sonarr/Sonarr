import React from 'react';
import { createRoot } from 'react-dom/client';
import createAppStore from 'Store/createAppStore';
import App from './App/App';

import 'Diag/ConsoleApi';

export async function bootstrap() {
  const store = createAppStore();
  const container = document.getElementById('root');

  const root = createRoot(container!);
  root.render(<App store={store} />);
}
