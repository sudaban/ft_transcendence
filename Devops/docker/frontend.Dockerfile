FROM node:20-alpine AS builder

WORKDIR /app

COPY package.json package-lock.json* ./

RUN npm install --prefer-offline --no-audit --no-fund --maxsockets=5 --legacy-peer-deps && \
    npm install @rolldown/binding-linux-x64-musl --prefer-offline --no-audit --no-fund --legacy-peer-deps

COPY . .

RUN npm run build && npm prune --production

FROM node:20-alpine

WORKDIR /app

COPY --from=builder /app/build ./build

COPY --from=builder /app/package.json ./

COPY --from=builder /app/node_modules ./node_modules

EXPOSE 3000

CMD ["node", "build/index.js"]
