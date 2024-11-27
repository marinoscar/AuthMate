-- Ensure the AccountType table exists and recreate it if necessary
DROP TABLE IF EXISTS AccountType CASCADE;
CREATE TABLE AccountType (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    Name VARCHAR(100) NOT NULL, -- Name of the account type
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for AccountType table
COMMENT ON TABLE AccountType IS 'Represents the type of an account in the system, such as Free, Tier1, or Tier2.';
COMMENT ON COLUMN AccountType.Id IS 'The unique identifier for the Account Type.';
COMMENT ON COLUMN AccountType.Name IS 'The name of the account type (e.g., Free, Tier1, Tier2).';
COMMENT ON COLUMN AccountType.UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN AccountType.CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN AccountType.UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN AccountType.UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN AccountType.Version IS 'The version of the record, incremented on updates.';

-- Insert a default row for the 'Free' account type
INSERT INTO AccountType (Name) 
VALUES ('Free');

-- Ensure the Account table exists and recreate it if necessary
DROP TABLE IF EXISTS Account CASCADE;
CREATE TABLE Account (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    AccountTypeId BIGINT NOT NULL REFERENCES AccountType(Id) ON DELETE CASCADE, -- Foreign key referencing AccountType
    Owner VARCHAR(255) NOT NULL, -- Owner of the account
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for Account table
COMMENT ON TABLE Account IS 'Represents an account in the system, with a reference to its type and owner information.';
COMMENT ON COLUMN Account.Id IS 'The unique identifier for the Account.';
COMMENT ON COLUMN Account.AccountTypeId IS 'The foreign key referencing the Account Type.';
COMMENT ON COLUMN Account.Owner IS 'The owner of the account (typically the user who created it).';
COMMENT ON COLUMN Account.UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN Account.CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN Account.UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN Account.UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN Account.Version IS 'The version of the record, incremented on updates.';

-- Ensure the User table exists and recreate it if necessary
DROP TABLE IF EXISTS "User" CASCADE;
CREATE TABLE "User" (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    Email VARCHAR(255) NOT NULL UNIQUE, -- Email address of the user
    ProviderKey VARCHAR(255) NOT NULL, -- Key from the authentication provider
    ProviderType VARCHAR(50) NOT NULL, -- Type of the authentication provider
    ProfilePictureUrl VARCHAR(500), -- URL to the user's profile picture
	UtcActiveUntil TIMESTAMP, -- Indicates the UTC data in which the user is active in the system
    Metadata TEXT, -- Metadata in JSON format
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for User table
COMMENT ON TABLE "User" IS 'Represents a user in the system, storing authentication and profile information.';
COMMENT ON COLUMN "User".Id IS 'The unique identifier for the User.';
COMMENT ON COLUMN "User".Email IS 'The email address of the user.';
COMMENT ON COLUMN "User".ProviderKey IS 'The unique key provided by the authentication provider (e.g., Google, Microsoft).';
COMMENT ON COLUMN "User".ProviderType IS 'The type of the authentication provider (e.g., Google, Microsoft, Facebook).';
COMMENT ON COLUMN "User".ProfilePictureUrl IS 'The URL of the user''s profile picture.';
COMMENT ON COLUMN "User".UtcActiveUntil IS 'Indicates the UTC data in which the user is active in the system';
COMMENT ON COLUMN "User".Metadata IS 'Metadata for the user, stored as a JSON object in string format.';
COMMENT ON COLUMN "User".UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN "User".CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN "User".UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN "User".UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN "User".Version IS 'The version of the record, incremented on updates.';

-- Ensure the Role table exists and recreate it if necessary
DROP TABLE IF EXISTS Role CASCADE;
CREATE TABLE Role (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    Name VARCHAR(100) NOT NULL UNIQUE, -- Name of the role
    Description VARCHAR(500), -- A brief description of the role
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for Role table
COMMENT ON TABLE Role IS 'Represents a role in the system, such as Admin, User, or Manager.';
COMMENT ON COLUMN Role.Id IS 'The unique identifier for the Role.';
COMMENT ON COLUMN Role.Name IS 'The name of the role (e.g., Admin, User).';
COMMENT ON COLUMN Role.Description IS 'A brief description of the role and its responsibilities.';
COMMENT ON COLUMN Role.UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN Role.CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN Role.UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN Role.UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN Role.Version IS 'The version of the record, incremented on updates.';

-- Insert default rows for predefined roles
INSERT INTO Role (Name, Description) VALUES 
('Administrator', 'Full access to all features and settings in the system.'),
('Owner', 'Responsible for managing an organization or account.'),
('Member', 'Standard user with access to core system features.'),
('Visitor', 'Limited access for viewing purposes only.');

-- Ensure the UserRole table exists and recreate it if necessary
DROP TABLE IF EXISTS UserRole CASCADE;
CREATE TABLE UserRole (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    UserId BIGINT NOT NULL REFERENCES "User"(Id) ON DELETE CASCADE, -- Foreign key referencing User
    RoleId BIGINT NOT NULL REFERENCES Role(Id) ON DELETE CASCADE, -- Foreign key referencing Role
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for UserRole table
COMMENT ON TABLE UserRole IS 'Represents the relationship between a user and a role in the system.';
COMMENT ON COLUMN UserRole.Id IS 'The unique identifier for the UserRole relationship.';
COMMENT ON COLUMN UserRole.UserId IS 'The foreign key referencing the User table.';
COMMENT ON COLUMN UserRole.RoleId IS 'The foreign key referencing the Role table.';
COMMENT ON COLUMN UserRole.UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN UserRole.CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN UserRole.UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN UserRole.UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN UserRole.Version IS 'The version of the record, incremented on updates.';

-- Ensure the UserInAccount table exists and recreate it if necessary
DROP TABLE IF EXISTS UserInAccount CASCADE;
CREATE TABLE UserInAccount (
    Id BIGSERIAL PRIMARY KEY, -- Primary Key
    UserId BIGINT NOT NULL REFERENCES "User"(Id) ON DELETE CASCADE, -- Foreign key referencing User
    AccountId BIGINT NOT NULL REFERENCES Account(Id) ON DELETE CASCADE, -- Foreign key referencing Account
    UtcCreatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record creation timestamp
    CreatedBy VARCHAR(255), -- User who created the record
    UtcUpdatedOn TIMESTAMP NOT NULL DEFAULT NOW(), -- Record update timestamp
    UpdatedBy VARCHAR(255), -- User who updated the record
    Version INT NOT NULL DEFAULT 1 -- Record version
);

-- Add descriptions for UserInAccount table
COMMENT ON TABLE UserInAccount IS 'Represents the relationship between a user and an account in the system.';
COMMENT ON COLUMN UserInAccount.Id IS 'The unique identifier for the UserInAccount relationship.';
COMMENT ON COLUMN UserInAccount.UserId IS 'The foreign key referencing the User table.';
COMMENT ON COLUMN UserInAccount.AccountId IS 'The foreign key referencing the Account table.';
COMMENT ON COLUMN UserInAccount.UtcCreatedOn IS 'The UTC timestamp when the record was created.';
COMMENT ON COLUMN UserInAccount.CreatedBy IS 'The user who created the record.';
COMMENT ON COLUMN UserInAccount.UtcUpdatedOn IS 'The UTC timestamp when the record was last updated.';
COMMENT ON COLUMN UserInAccount.UpdatedBy IS 'The user who last updated the record.';
COMMENT ON COLUMN UserInAccount.Version IS 'The version of the record, incremented on updates.';

