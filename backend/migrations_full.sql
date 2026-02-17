CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112082558_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260112082558_InitialCreate', '10.0.1');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE TABLE students (
        id uuid NOT NULL,
        first_name character varying(50) NOT NULL,
        last_name character varying(50) NOT NULL,
        email character varying(100) NOT NULL,
        phone_number character varying(20),
        date_of_birth timestamp with time zone NOT NULL,
        address character varying(500),
        city character varying(50),
        state character varying(50),
        postal_code character varying(10),
        enrollment_number character varying(50) NOT NULL,
        enrollment_date timestamp with time zone NOT NULL,
        is_active boolean NOT NULL,
        guardian_name character varying(100),
        guardian_phone character varying(20),
        guardian_email character varying(100),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        created_by text,
        updated_by text,
        CONSTRAINT "PK_students" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        username character varying(50) NOT NULL,
        email character varying(100) NOT NULL,
        password_hash text NOT NULL,
        first_name character varying(50) NOT NULL,
        last_name character varying(50) NOT NULL,
        role integer NOT NULL,
        is_active boolean NOT NULL,
        refresh_token text,
        refresh_token_expiry_time timestamp with time zone,
        last_login_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        created_by text,
        updated_by text,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE INDEX "IX_students_city" ON students (city);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE UNIQUE INDEX "IX_students_email" ON students (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE UNIQUE INDEX "IX_students_enrollment_number" ON students (enrollment_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE INDEX "IX_students_is_active" ON students (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE UNIQUE INDEX "IX_users_email" ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    CREATE UNIQUE INDEX "IX_users_username" ON users (username);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260112132741_AddUsersAndStudents') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260112132741_AddUsersAndStudents', '10.0.1');
    END IF;
END $EF$;
COMMIT;

