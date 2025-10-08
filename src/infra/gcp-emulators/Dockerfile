FROM asia.gcr.io/google.com/cloudsdktool/google-cloud-cli:542.0.0-emulators

COPY background.sh /config/
COPY init-pubsub.sh /config/
# COPY verify-pubsub.sh /config/

RUN chmod 777 /config/background.sh
RUN chmod 777 /config/init-pubsub.sh
# RUN chmod 777 /config/verify-pubsub.sh

# CMD /config/background.sh