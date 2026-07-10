FROM docker:cli

COPY autoheal.sh /autoheal.sh
RUN chmod +x /autoheal.sh

ENTRYPOINT ["/autoheal.sh"]
