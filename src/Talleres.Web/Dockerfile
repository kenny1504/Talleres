FROM node:24-alpine AS dependencias
WORKDIR /aplicacion

COPY src/Talleres.Web/package.json src/Talleres.Web/package-lock.json ./
RUN npm ci

FROM dependencias AS compilacion
WORKDIR /aplicacion

COPY src/Talleres.Web/ ./

ARG NEXT_PUBLIC_API_URL=/backend
ARG NEXT_PUBLIC_EMPRESA_ID=1
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_EMPRESA_ID=$NEXT_PUBLIC_EMPRESA_ID

RUN npm run build

FROM node:24-alpine AS dependencias-produccion
WORKDIR /aplicacion

COPY src/Talleres.Web/package.json src/Talleres.Web/package-lock.json ./
RUN npm ci --omit=dev && npm cache clean --force

FROM node:24-alpine AS final
WORKDIR /aplicacion

ENV NODE_ENV=production
ENV PORT=3000
ENV HOST=0.0.0.0
EXPOSE 3000

COPY --from=dependencias-produccion --chown=node:node /aplicacion/node_modules ./node_modules
COPY --from=compilacion --chown=node:node /aplicacion/dist ./dist
COPY --chown=node:node src/Talleres.Web/package.json src/Talleres.Web/servidor.mjs ./

USER node

CMD ["node", "servidor.mjs"]
