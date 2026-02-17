#!/bin/bash
set -e

# Create pg_hba.conf with trust auth for localhost
cat > /var/lib/postgresql/data/pg_hba.conf << EOF
# PostgreSQL Client Authentication Configuration
# TYPE  DATABASE        USER            ADDRESS                 METHOD
local   all             all                                     trust
host    all             all             127.0.0.1/32            trust
host    all             all             ::1/128                 trust
host    all             all             0.0.0.0/0               password
local   replication     all                                     trust
host    replication     all             127.0.0.1/32            trust
host    replication     all             ::1/128                 trust
EOF

chmod 600 /var/lib/postgresql/data/pg_hba.conf
