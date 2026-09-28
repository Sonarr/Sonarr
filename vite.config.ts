import path from 'path';
import { spawn, ChildProcess } from 'child_process';
import { cpSync, mkdirSync, readdirSync } from 'fs';
import react from '@vitejs/plugin-react';
import { defineConfig, Plugin } from 'vite';
import { patchCssModules } from 'vite-css-modules';

const src = path.resolve(__dirname, 'frontend/src');
const outDir = path.resolve(__dirname, '_output/UI');

const contentDir = path.join(src, 'Content');
const themeCss = path.join(src, 'Styles/Themes/themes.css');

function htmlFiles() {
  return readdirSync(src).filter((name) => name.endsWith('.html'));
}

function copyStaticContent(): Plugin {
  function copyAll() {
    mkdirSync(outDir, { recursive: true });

    cpSync(contentDir, path.join(outDir, 'Content'), { recursive: true });
    cpSync(themeCss, path.join(outDir, 'Content', 'theme.css'));

    for (const file of htmlFiles()) {
      cpSync(path.join(src, file), path.join(outDir, file));
    }
  }

  function owns(file: string) {
    return (
      file.startsWith(contentDir) ||
      file === themeCss ||
      (path.dirname(file) === src && file.endsWith('.html'))
    );
  }

  return {
    name: 'copy-static-content',

    buildStart() {
      for (const file of htmlFiles()) {
        this.addWatchFile(path.join(src, file));
      }

      this.addWatchFile(contentDir);
      this.addWatchFile(themeCss);
    },

    closeBundle() {
      copyAll();
    },

    configureServer(server) {
      copyAll();

      server.watcher.add([contentDir, themeCss, path.join(src, '*.html')]);

      server.watcher.on('add', (file) => owns(file) && copyAll());
      server.watcher.on('change', (file) => owns(file) && copyAll());
    },
  };
}

function cssModuleTypes(): Plugin {
  let child: ChildProcess | undefined;

  function stop() {
    child?.kill();
    child = undefined;
  }

  return {
    name: 'css-module-types',
    apply: 'serve',

    configureServer(server) {
      const bin = process.platform === 'win32' ? 'tcm.cmd' : 'tcm';

      child = spawn(
        path.join(__dirname, 'node_modules', '.bin', bin),
        ['frontend/src', '--camelCase', '--pattern', '**/*.module.css', '--watch'],
        { cwd: __dirname, stdio: 'inherit' }
      );

      server.httpServer?.on('close', stop);
      process.on('exit', stop);
    },
  };
}

const srcAliases = Object.fromEntries(
  readdirSync(src, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => [entry.name, path.join(src, entry.name)])
);

export default defineConfig({
  plugins: [
    patchCssModules({ exportMode: 'default' }),
    react(),
    copyStaticContent(),
    cssModuleTypes(),
  ],

  base: '/',

  server: {
    port: Number(process.env.SONARR_VITE_PORT ?? 8959),
    strictPort: true,
    hmr: {
      clientPort: Number(process.env.SONARR_VITE_PORT ?? 8959),
    },
  },

  define: {
    __DEV__: JSON.stringify(process.env.NODE_ENV !== 'production'),
  },

  build: {
    outDir,
    emptyOutDir: true,
    target: 'esnext',
    sourcemap: true,
  },

  experimental: {
    renderBuiltUrl(filename, { hostType }) {
      if (hostType === 'html') {
        return `/${filename}`;
      }

      return { relative: true };
    },
  },

  resolve: {
    alias: {
      ...srcAliases,
      jquery: 'jquery/dist/jquery.min.js',
    },
  },

  css: {
    postcss: path.resolve(__dirname, 'frontend'),
  },
});
