import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App/App';

import 'Diag/ConsoleApi';

export async function bootstrap() {
  document.title = window.Sonarr.instanceName;

  const container = document.getElementById('root');

  const root = createRoot(container!);
  root.render(<App />);
}
