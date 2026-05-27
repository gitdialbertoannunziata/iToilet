FROM node:19-alpine as base
WORKDIR /app
EXPOSE 8080

FROM node:19-alpine as build
RUN mkdir src
COPY . ./src
WORKDIR /src/client
RUN npm install 
RUN npm run build
WORKDIR /src
RUN npm install

FROM base 
WORKDIR /app
COPY --from=build /src .

CMD node main.js