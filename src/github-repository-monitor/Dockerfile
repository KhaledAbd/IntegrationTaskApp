# Build stage
FROM node:20 AS build
WORKDIR /app

# Copy package.json and install dependencies
COPY src/github-repository-monitor/package*.json ./
RUN npm install

# Copy source and build
COPY src/github-repository-monitor/ .
RUN npx kendo-ui-license activate
RUN npm run build

# Final stage
FROM nginx:alpine
# Copy the built files to the nginx public folder
COPY --from=build /app/dist/github-repository-monitor/browser /usr/share/nginx/html
# Copy nginx configuration
COPY src/github-repository-monitor/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
