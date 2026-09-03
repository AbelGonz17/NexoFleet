# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies
COPY package.json package-lock.json ./
RUN npm install --legacy-peer-deps

# Copy source and build Nuxt application
COPY . .
RUN npm run build

# Runtime stage
FROM node:20-alpine AS final
WORKDIR /app

ENV NODE_ENV=production
ENV NITRO_HOST=0.0.0.0
ENV NITRO_PORT=3000

EXPOSE 3000

COPY --from=build /app/.output ./.output

CMD ["node", ".output/server/index.mjs"]
