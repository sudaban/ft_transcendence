FROM grafana/grafana:latest

USER root
RUN chown -R 472:472 /usr/share/grafana/data/plugins-bundled

COPY --chown=472:472 provisioning/datasources /etc/grafana/provisioning/datasources
COPY --chown=472:472 provisioning/dashboards /etc/grafana/provisioning/dashboards
COPY --chown=472:472 dashboards /etc/grafana/dashboards

USER grafana
