-- Drop the table if it exists
DROP TABLE IF EXISTS "AccountType" CASCADE;

-- Create the table
CREATE TABLE "AccountType" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE,
    "UtcCreatedOn" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" VARCHAR NULL,
    "UtcUpdatedOn" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedBy" VARCHAR NULL,
    "Version" INTEGER NOT NULL DEFAULT 0
);

-- Add comments for the table and columns
COMMENT ON TABLE "AccountType" IS 'Represents the type of an account in the system, such as Free, Tier1, or Tier2.';
COMMENT ON COLUMN "AccountType"."Id" IS 'The unique identifier for the Account Type.';
COMMENT ON COLUMN "AccountType"."Name" IS 'The name of the account type (e.g., Free, Tier1, Tier2).';
COMMENT ON COLUMN "AccountType"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "AccountType"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "AccountType"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "AccountType"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "AccountType"."Version" IS 'The version of the record, incremented on updates.';

-- Insert a default row for the 'Free' account type
INSERT INTO "AccountType" ("Name") 
VALUES ('Free');



-- Drop the table if it exists
DROP TABLE IF EXISTS "Account" CASCADE;

-- Create the table
CREATE TABLE "Account" (
    "Id" BIGSERIAL PRIMARY KEY,
    "AccountTypeId" BIGINT NOT NULL,
    "Owner" VARCHAR(255) NOT NULL UNIQUE,
    "Name" VARCHAR NULL UNIQUE,
    "Description" TEXT NULL,
    "UtcCreatedOn" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR NULL,
    "UtcUpdatedOn" TIMESTAMP NOT NULL,
    "UpdatedBy" VARCHAR NULL,
    "Version" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_accounttype FOREIGN KEY ("AccountTypeId") REFERENCES "AccountType" ("Id") ON DELETE CASCADE
);

-- Add comments for the table and columns
COMMENT ON TABLE "Account" IS 'Represents an account in the system, with a reference to its type and owner information.';
COMMENT ON COLUMN "Account"."Id" IS 'The unique identifier for the Account.';
COMMENT ON COLUMN "Account"."AccountTypeId" IS 'The foreign key referencing the Account Type.';
COMMENT ON COLUMN "Account"."Owner" IS 'The owner of the account (typically the user who created it).';
COMMENT ON COLUMN "Account"."Name" IS 'The name of the account.';
COMMENT ON COLUMN "Account"."Description" IS 'A description of the account.';
COMMENT ON COLUMN "Account"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "Account"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "Account"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "Account"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "Account"."Version" IS 'The version of the record, incremented on updates.';


-- Drop the table if it exists
DROP TABLE IF EXISTS "AppUser" CASCADE;

-- Create the table
CREATE TABLE "AppUser" (
    "Id" BIGSERIAL PRIMARY KEY,
    "DisplayName" VARCHAR(255),
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "ProviderKey" VARCHAR(255) NOT NULL,
    "ProviderType" VARCHAR(50) NOT NULL,
    "ProfilePictureUrl" VARCHAR(500),
    "UtcActiveUntil" TIMESTAMP,
    "Metadata" TEXT,
    "AccountId" BIGINT NOT NULL,
    "UtcCreatedOn" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR,
    "UtcUpdatedOn" TIMESTAMP NOT NULL,
    "UpdatedBy" VARCHAR,
    "Version" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_account FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id") ON DELETE CASCADE
);

-- Add comments for the table and columns
COMMENT ON TABLE "AppUser" IS 'Represents a user in the system, storing authentication and profile information.';
COMMENT ON COLUMN "AppUser"."Id" IS 'The unique identifier for the User.';
COMMENT ON COLUMN "AppUser"."DisplayName" IS 'The name of the application user.';
COMMENT ON COLUMN "AppUser"."Email" IS 'The email address of the user.';
COMMENT ON COLUMN "AppUser"."ProviderKey" IS 'The unique key provided by the authentication provider (e.g., Google, Microsoft).';
COMMENT ON COLUMN "AppUser"."ProviderType" IS 'The type of the authentication provider (e.g., Google, Microsoft, Facebook).';
COMMENT ON COLUMN "AppUser"."ProfilePictureUrl" IS 'The URL of the user''s profile picture.';
COMMENT ON COLUMN "AppUser"."UtcActiveUntil" IS 'Indicates the UTC date until which the user is active in the system.';
COMMENT ON COLUMN "AppUser"."Metadata" IS 'Metadata for the user, stored as a JSON object in string format.';
COMMENT ON COLUMN "AppUser"."AccountId" IS 'The foreign key referencing the associated Account.';
COMMENT ON COLUMN "AppUser"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "AppUser"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "AppUser"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "AppUser"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "AppUser"."Version" IS 'The version of the record, incremented on updates.';

-- Drop the table if it exists
DROP TABLE IF EXISTS "Role" CASCADE;

-- Create the table
CREATE TABLE "Role" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE,
    "Description" VARCHAR(500),
    "UtcCreatedOn" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" VARCHAR NULL,
    "UtcUpdatedOn" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedBy" VARCHAR NULL,
    "Version" INTEGER NOT NULL DEFAULT 0
);

-- Add comments for the table and columns
COMMENT ON TABLE "Role" IS 'Represents a role in the system, such as Admin, User, or Manager.';
COMMENT ON COLUMN "Role"."Id" IS 'The unique identifier for the Role.';
COMMENT ON COLUMN "Role"."Name" IS 'The name of the role (e.g., Admin, User).';
COMMENT ON COLUMN "Role"."Description" IS 'A brief description of the role and its responsibilities.';
COMMENT ON COLUMN "Role"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "Role"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "Role"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "Role"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "Role"."Version" IS 'The version of the record, incremented on updates.';


-- Insert default rows for predefined roles
INSERT INTO "Role" ("Name", "Description") VALUES 
('Administrator', 'Full access to all features and settings in the system.'),
('Owner', 'Responsible for managing an organization or account.'),
('Member', 'Standard user with access to core system features.'),
('Visitor', 'Limited access for viewing purposes only.');

-- Drop the table if it exists
DROP TABLE IF EXISTS "AppUserRole" CASCADE;

-- Create the table
CREATE TABLE "AppUserRole" (
    "Id" BIGSERIAL PRIMARY KEY,
    "AppUserId" BIGINT NOT NULL,
    "RoleId" BIGINT NOT NULL,
    "UtcCreatedOn" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR NULL,
    "UtcUpdatedOn" TIMESTAMP NOT NULL,
    "UpdatedBy" VARCHAR NULL,
    "Version" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_appuser FOREIGN KEY ("AppUserId") REFERENCES "AppUser" ("Id") ON DELETE CASCADE,
    CONSTRAINT fk_role FOREIGN KEY ("RoleId") REFERENCES "Role" ("Id") ON DELETE CASCADE
);

-- Add comments for the table and columns
COMMENT ON TABLE "AppUserRole" IS 'Represents the relationship between a user and a role in the system.';
COMMENT ON COLUMN "AppUserRole"."Id" IS 'The unique identifier for the UserRole relationship.';
COMMENT ON COLUMN "AppUserRole"."AppUserId" IS 'The foreign key referencing the User table.';
COMMENT ON COLUMN "AppUserRole"."RoleId" IS 'The foreign key referencing the Role table.';
COMMENT ON COLUMN "AppUserRole"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "AppUserRole"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "AppUserRole"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "AppUserRole"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "AppUserRole"."Version" IS 'The version of the record, incremented on updates.';
