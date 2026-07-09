FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files and install dependencies
COPY package.json package-lock.json ./
RUN npm install --prefer-offline --no-audit --no-fund --maxsockets=5

# Copy the rest of the application code
COPY . .

# Build the SvelteKit app
RUN npm run build

# Expose the default port (Node adapter uses 3000 by default)
EXPOSE 3000

# Start the built app
CMD ["node", "build/index.js"]