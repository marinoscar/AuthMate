
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
	"UtcExpirationDate" TIMESTAMP NULL,
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
COMMENT ON COLUMN "Account"."UtcExpirationDate" IS 'The UTC timestamp when account will expire.';
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
	"OAuthAccessToken" VARCHAR(500) NULL,
	"OAuthTokenType" VARCHAR(500) NULL,
	"OAuthRefreshToken" VARCHAR(500) NULL,
	"OAuthTokenUtcExpiresIn" TIMESTAMP NULL,
    "UtcActiveUntil" TIMESTAMP,
	"UtcLastLogin" TIMESTAMP,
	"Timezone" VARCHAR(100),
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
COMMENT ON COLUMN "AppUser"."OAuthAccessToken" IS 'The access token issued by the OAuth provider.';
COMMENT ON COLUMN "AppUser"."OAuthTokenType" IS 'OAuth provider token type, for example bearing';
COMMENT ON COLUMN "AppUser"."OAuthRefreshToken" IS 'OAuth refresh token that applications can use to obtain another access token if tokens can expire.';
COMMENT ON COLUMN "AppUser"."OAuthTokenUtcExpiresIn" IS 'OAuth validatity lifetime of the token.';
COMMENT ON COLUMN "AppUser"."ProfilePictureUrl" IS 'The URL of the user''s profile picture.';
COMMENT ON COLUMN "AppUser"."UtcActiveUntil" IS 'Indicates the UTC date until which the user is active in the system.';
COMMENT ON COLUMN "AppUser"."UtcLastLogin" IS 'Indicates the UTC date and time that the user was authenticated.';
COMMENT ON COLUMN "AppUser"."Timezone" IS 'The timezone the user has to resolve UTC dates.';
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

-- Drop the table if it exists
DROP TABLE IF EXISTS "AppUserLoginHistory" CASCADE;

-- Create the AppUserLoginHistory table
CREATE TABLE "AppUserLoginHistory" (
    "Id" BIGSERIAL PRIMARY KEY,
    "UtcLogIn" TIMESTAMP NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "OS" VARCHAR(128) NOT NULL,
    "IpAddress" VARCHAR(45) NOT NULL,
    "Browser" VARCHAR(128) NOT NULL
);

-- Create an index on the Email column
CREATE INDEX "IX_AppUserLoginHistory_Email" ON "AppUserLoginHistory" ("Email");

-- Add comments for the table and its columns
COMMENT ON TABLE "AppUserLoginHistory" IS 'Tracks all application user login events in the system.';
COMMENT ON COLUMN "AppUserLoginHistory"."Id" IS 'The unique identifier for the login entry.';
COMMENT ON COLUMN "AppUserLoginHistory"."UtcLogIn" IS 'The UTC timestamp when the login occurred.';
COMMENT ON COLUMN "AppUserLoginHistory"."Email" IS 'The email of the user who logged in.';
COMMENT ON COLUMN "AppUserLoginHistory"."OS" IS 'The operating system of the device used for login.';
COMMENT ON COLUMN "AppUserLoginHistory"."IpAddress" IS 'The IP address from which the login occurred.';
COMMENT ON COLUMN "AppUserLoginHistory"."Browser" IS 'The browser of the device used for login.';

-- Drop the table if it exists
DROP TABLE IF EXISTS "InviteToApplication" CASCADE;

-- Create the table
CREATE TABLE "InviteToApplication" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "AccountTypeId" BIGINT NOT NULL,
    "UtcExpiration" TIMESTAMP NOT NULL,
    "UserMessage" VARCHAR(1024),
    "UtcAcceptedOn" TIMESTAMP,
    "UtcRejectedOn" TIMESTAMP,
    "RejectedReason" VARCHAR(512),
    "UtcCreatedOn" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR NULL,
    "UtcUpdatedOn" TIMESTAMP NOT NULL,
    "UpdatedBy" VARCHAR NULL,
    "Version" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_accounttype FOREIGN KEY ("AccountTypeId") REFERENCES "AccountType" ("Id") ON DELETE CASCADE
);

-- Add comments for the table and columns
COMMENT ON TABLE "InviteToApplication" IS 'Represents a pre-authorized user who has the ability to create an account in the system.';
COMMENT ON COLUMN "InviteToApplication"."Id" IS 'The unique identifier for the invitation.';
COMMENT ON COLUMN "InviteToApplication"."Email" IS 'The email address of the pre-authorized user.';
COMMENT ON COLUMN "InviteToApplication"."UtcExpiration" IS 'The UTC timestamp when the invitation expires.';
COMMENT ON COLUMN "InviteToApplication"."UserMessage" IS 'The message included in the invitation.';
COMMENT ON COLUMN "InviteToApplication"."UtcAcceptedOn" IS 'The UTC timestamp when the invitation was accepted.';
COMMENT ON COLUMN "InviteToApplication"."UtcRejectedOn" IS 'The UTC timestamp when the invitation was rejected.';
COMMENT ON COLUMN "InviteToApplication"."RejectedReason" IS 'The reason provided for rejecting the invitation.';
COMMENT ON COLUMN "InviteToApplication"."AccountTypeId" IS 'The foreign key referencing the AccountType entity.';
COMMENT ON COLUMN "InviteToApplication"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "InviteToApplication"."CreatedBy" IS 'The user who created the record.';
COMMENT ON COLUMN "InviteToApplication"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "InviteToApplication"."UpdatedBy" IS 'The user who last updated the record.';
COMMENT ON COLUMN "InviteToApplication"."Version" IS 'The version of the record, incremented on updates.';

-- Adds one user to the list
INSERT INTO "InviteToApplication" ("Email", "AccountTypeId", "UtcExpiration", "UtcCreatedOn", "UtcUpdatedOn", "Version")
VALUES (
    'oscar.marin.saenz@gmail.com',
    (SELECT "Id" FROM "AccountType" WHERE "Name" = 'Free'),
	(NOW() + INTERVAL '1 year'),                     -- UtcExpiration
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    0
);

-- Drop the table if it exists
DROP TABLE IF EXISTS "InviteToAccount" CASCADE;

-- Create the InviteToAccount table
CREATE TABLE "InviteToAccount" (
    "Id" BIGSERIAL PRIMARY KEY,
    "AccountId" BIGINT NOT NULL,
    "Email" VARCHAR(256) NOT NULL UNIQUE,
    "UtcExpiration" TIMESTAMP NOT NULL,
    "UserMessage" VARCHAR(1024),
    "UtcAcceptedOn" TIMESTAMP,
    "UtcRejectedOn" TIMESTAMP,
    "RejectedReason" VARCHAR(512),
    "RoleId" BIGINT,
    "UtcCreatedOn" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(256),
    "UtcUpdatedOn" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" VARCHAR(256),
    "Version" INTEGER NOT NULL DEFAULT 1,

    -- Foreign Key Constraints
    CONSTRAINT "FK_InviteToAccount_Account" FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_InviteToAccount_Role" FOREIGN KEY ("RoleId") REFERENCES "Role" ("Id") ON DELETE SET NULL
);

-- Create an index on the Email column
CREATE INDEX "IX_InviteToAccount_Email" ON "InviteToAccount" ("Email");

/*
-- Add a row to the table
INSERT INTO "InviteToAccount" (
    "Email",
    "UtcExpiration",
    "AccountId",
    "RoleId"
) VALUES (
    'oscar@marin.cr',                                -- Email
    (NOW() + INTERVAL '1 year'),                     -- UtcExpiration
    (SELECT "Id" FROM "Account" WHERE "Owner" = 'oscar.marin.saenz@gmail.com'), -- AccountId
    (SELECT "Id" FROM "Role" WHERE "Name" = 'Owner') -- RoleId
);
*/

-- Add comments for the table and its columns
COMMENT ON TABLE "InviteToAccount" IS 'Tracks invitations sent to users to join an account with specific roles.';
COMMENT ON COLUMN "InviteToAccount"."Id" IS 'The unique identifier for the invitation.';
COMMENT ON COLUMN "InviteToAccount"."AccountId" IS 'The foreign key to the Account table.';
COMMENT ON COLUMN "InviteToAccount"."Email" IS 'The email address of the invitee.';
COMMENT ON COLUMN "InviteToAccount"."UtcExpiration" IS 'The UTC timestamp when the invitation expires.';
COMMENT ON COLUMN "InviteToAccount"."UserMessage" IS 'The message included in the invitation.';
COMMENT ON COLUMN "InviteToAccount"."UtcAcceptedOn" IS 'The UTC timestamp when the invitation was accepted.';
COMMENT ON COLUMN "InviteToAccount"."UtcRejectedOn" IS 'The UTC timestamp when the invitation was rejected.';
COMMENT ON COLUMN "InviteToAccount"."RejectedReason" IS 'The reason provided for rejecting the invitation.';
COMMENT ON COLUMN "InviteToAccount"."RoleId" IS 'The foreign key to the Role table, nullable.';
COMMENT ON COLUMN "InviteToAccount"."UtcCreatedOn" IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "InviteToAccount"."CreatedBy" IS 'The identifier of the user who created the record.';
COMMENT ON COLUMN "InviteToAccount"."UtcUpdatedOn" IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "InviteToAccount"."UpdatedBy" IS 'The identifier of the user who last updated the record.';
COMMENT ON COLUMN "InviteToAccount"."Version" IS 'The version of the record for concurrency handling.';
