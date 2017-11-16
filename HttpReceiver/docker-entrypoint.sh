#!/usr/bin/env sh

HOST=${HOST:-127.0.0.1}
PORT=${PORT:-8080}
MQTT_HOST=${MQTT_HOST:-mqtt}
MQTT_PORT=${MQTT_PORT:-1833}
PG_USER=${PG_USER:-postgres}
PG_PASS=${PG_PASS:-password}
PG_HOST=${PG_HOST:-postgres}
PG_PORT=${PG_PORT:-5432}
PG_DB=${PG_DB:-postgres}

exec /App/HttpReceiver --host ${HOST} --port ${PORT} --mqttbroker ${MQTT_HOST} ${MQTT_PORT} --postgres ${PG_USER} ${PG_PASS} ${PG_HOST} ${PG_PORT} ${PG_DB}