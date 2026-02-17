# Frontend Dockerfile - Multi-stage build for React + Vite

# Stage 1: Development with hot reload
FROM node:20-alpine AS development
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm ci

# Copy application files (will be overridden by volume mount)
COPY . .

# Expose Vite dev server port
EXPOSE 5173

# Start dev server with host flag for Docker
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

# Stage 2: Build for production
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci

# Copy all source files
COPY . .

# Build the application
RUN npm run build

# Stage 3: Production with nginx
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Remove default nginx content
RUN rm -rf ./*

# Copy built assets from build stage
COPY --from=build /app/dist .

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
