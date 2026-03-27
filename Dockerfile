FROM node:20 AS ui-build
WORKDIR /src

# Install dependencies separately for better layer caching
COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

# Copy frontend source and build
COPY frontend/ frontend/
COPY tsconfig.json ./
RUN yarn build --env production

# Use the official linuxserver image as base — all config, volumes, PUID/PGID work unchanged
FROM lscr.io/linuxserver/sonarr:latest
COPY --from=ui-build /src/_output/UI/ /app/sonarr/bin/UI/
