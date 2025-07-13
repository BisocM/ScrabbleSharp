# build the app
FROM node:18-alpine AS build

WORKDIR /app

# install dependencies
COPY package.json package-lock.json ./
RUN npm ci

COPY .env.production ./

# copy sources and build
COPY . .
RUN npm run build

# serve the built files
FROM nginx:stable-alpine

# remove default content
RUN rm -rf /usr/share/nginx/html/*

# copy build artifacts
COPY --from=build /app/dist/ /usr/share/nginx/html/

# expose HTTP port
EXPOSE 80

# run nginx in foreground
CMD ["nginx", "-g", "daemon off;"]