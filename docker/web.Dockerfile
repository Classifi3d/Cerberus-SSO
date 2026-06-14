FROM node:20 AS build
WORKDIR /app

COPY CerberusSSOApplication_FrontEnd/mfa-webapp/ .

RUN npm install
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/mfa-webapp/browser /usr/share/nginx/html