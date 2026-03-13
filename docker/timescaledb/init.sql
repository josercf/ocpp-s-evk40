-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb;

-- Registered charge points
CREATE TABLE IF NOT EXISTS charge_points (
    charge_point_id VARCHAR(64) PRIMARY KEY,
    vendor VARCHAR(64),
    model VARCHAR(64),
    serial_number VARCHAR(64),
    firmware_version VARCHAR(64),
    registered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_heartbeat TIMESTAMPTZ
);

-- Charging sessions (transactions)
CREATE TABLE IF NOT EXISTS charging_sessions (
    transaction_id SERIAL PRIMARY KEY,
    charge_point_id VARCHAR(64) REFERENCES charge_points(charge_point_id),
    connector_id INT NOT NULL,
    id_tag VARCHAR(20),
    start_time TIMESTAMPTZ NOT NULL,
    stop_time TIMESTAMPTZ,
    meter_start INT NOT NULL,
    meter_stop INT,
    energy_kwh DOUBLE PRECISION,
    stop_reason VARCHAR(32)
);

-- Current connector status
CREATE TABLE IF NOT EXISTS connector_status (
    charge_point_id VARCHAR(64) REFERENCES charge_points(charge_point_id),
    connector_id INT NOT NULL,
    status VARCHAR(32) NOT NULL,
    error_code VARCHAR(32),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (charge_point_id, connector_id)
);

-- Time series meter values (hypertable)
CREATE TABLE IF NOT EXISTS meter_values (
    time TIMESTAMPTZ NOT NULL,
    charge_point_id VARCHAR(64) NOT NULL,
    connector_id INT NOT NULL,
    transaction_id INT,
    measurand VARCHAR(64) NOT NULL,
    value DOUBLE PRECISION NOT NULL,
    unit VARCHAR(16),
    phase VARCHAR(8)
);

-- Convert to hypertable (partitioned by time)
SELECT create_hypertable('meter_values', 'time', if_not_exists => TRUE);

-- Indexes for efficient querying
CREATE INDEX IF NOT EXISTS idx_meter_values_cp ON meter_values (charge_point_id, time DESC);
CREATE INDEX IF NOT EXISTS idx_meter_values_txn ON meter_values (transaction_id, time DESC);
CREATE INDEX IF NOT EXISTS idx_meter_values_measurand ON meter_values (measurand, time DESC);

-- EF Core migrations tracking table (so EF doesn't try to re-create tables)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
