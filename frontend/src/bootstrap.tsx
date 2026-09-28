import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App/App';

import 'Diag/ConsoleApi';

export async function bootstrap() {
  const container = document.getElementById('root');

  const root = createRoot(container!);
  root.render(<App />);
}
