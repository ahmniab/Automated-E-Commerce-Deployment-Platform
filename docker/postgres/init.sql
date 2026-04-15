CREATE DATABASE catalogdb;
CREATE DATABASE orderingdb;

\connect identitydb
CREATE EXTENSION IF NOT EXISTS vector;

\connect catalogdb
CREATE EXTENSION IF NOT EXISTS vector;

\connect orderingdb
CREATE EXTENSION IF NOT EXISTS vector;
