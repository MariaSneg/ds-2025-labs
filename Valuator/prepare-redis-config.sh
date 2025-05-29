#!/bin/bash
mkdir -p ./redis/generated
envsubst < ./redis/redis.conf.template > ./redis/generated/redis.conf