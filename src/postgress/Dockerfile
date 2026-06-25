FROM ankane/pgvector 

ENV POSTGRES_USER=eshop
ENV POSTGRES_DB=identitydb

COPY ./init.sql /docker-entrypoint-initdb.d/01-init.sql