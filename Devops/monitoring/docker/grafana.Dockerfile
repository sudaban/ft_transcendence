FROM grafana/grafana:latest

COPY provisioning/datasources /etc/grafana/provisioning/datasources
COPY provisioning/dashboards /etc/grafana/provisioning/dashboards
COPY dashboards /etc/grafana/dashboards

USER grafana
