-- Migration: Add School Configuration Table
-- Date: 2026-03-05
-- Purpose: Create schools table for multi-tenant configuration (separate installations per school)

CREATE TABLE schools (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    code VARCHAR(50) UNIQUE NOT NULL,
    address VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    postal_code VARCHAR(20),
    phone_number VARCHAR(20),
    email_address VARCHAR(255),
    website VARCHAR(255),
    logo_image BYTEA,
    logo_file_name VARCHAR(255),
    established_date TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT true,
    primary_color VARCHAR(7) DEFAULT '#1976D2',
    secondary_color VARCHAR(7) DEFAULT '#DC004E',
    accent_color VARCHAR(7) DEFAULT '#FF6F00',
    header_text TEXT,
    footer_text TEXT,
    date_format VARCHAR(20) DEFAULT 'dd/MM/yyyy',
    currency_code VARCHAR(3) DEFAULT 'INR',
    currency_symbol VARCHAR(5) DEFAULT '₹',
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    created_by VARCHAR(255),
    updated_by VARCHAR(255),
    CONSTRAINT school_email_unique UNIQUE (email_address)
);

-- Create indexes for performance
CREATE INDEX idx_schools_code ON schools(code);
CREATE INDEX idx_schools_email ON schools(email_address);
CREATE INDEX idx_schools_is_active ON schools(is_active);
