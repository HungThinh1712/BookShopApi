FROM mongo:4.4
COPY ./keyfile /etc/mongodb/keyfile
COPY ./mongod.conf /etc/mongod.conf
RUN chmod 400 /etc/mongodb/keyfile
RUN chown 999 /etc/mongodb/keyfile
