#!/bin/sh
# Autoheal - monitors and restarts unhealthy containers
INTERVAL=${AUTOHEAL_INTERVAL:-30}
START_PERIOD=${AUTOHEAL_START_PERIOD:-60}

echo "Autoheal starting... waiting ${START_PERIOD}s for containers to initialize."
sleep "$START_PERIOD"
echo "Autoheal active. Checking every ${INTERVAL}s for unhealthy containers."

while true; do
  UNHEALTHY=$(docker ps --filter "label=autoheal=true" --filter "health=unhealthy" --format "{{.Names}}")
  if [ -n "$UNHEALTHY" ]; then
    for CONTAINER in $UNHEALTHY; do
      echo "$(date '+%Y-%m-%d %H:%M:%S') [AUTOHEAL] Restarting unhealthy container: $CONTAINER"
      docker restart "$CONTAINER"
    done
  fi
  sleep "$INTERVAL"
done
