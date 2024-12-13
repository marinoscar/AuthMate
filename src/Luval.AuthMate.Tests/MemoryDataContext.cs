using Luval.AuthMate.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Tests
{
    public class MemoryDataContext
    {

        public static List<AppUser> GenerateUsers()
        {
            return new List<AppUser>
        {
            new AppUser
            {
                DisplayName = "Alice Johnson",
                Email = "alice.johnson@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/alice.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"admin\"}",
                AccountId = 1,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new AppUser
            {
                DisplayName = "Bob Smith",
                Email = "bob.smith@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/bob.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"user\"}",
                AccountId = 2,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new AppUser
            {
                DisplayName = "Charlie Brown",
                Email = "charlie.brown@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/charlie.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"moderator\"}",
                AccountId = 3,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new AppUser
            {
                DisplayName = "Diana Prince",
                Email = "diana.prince@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/diana.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"admin\"}",
                AccountId = 4,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new AppUser
            {
                DisplayName = "Eve Adams",
                Email = "eve.adams@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/eve.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"user\"}",
                AccountId = 5,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new AppUser
            {
                DisplayName = "Frank Castle",
                Email = "frank.castle@example.com",
                ProviderKey = Guid.NewGuid().ToString(),
                ProviderType = "Google",
                ProfilePictureUrl = "https://example.com/profiles/frank.jpg",
                UtcActiveUntil = DateTime.UtcNow.AddYears(1),
                UtcLastLogin = DateTime.UtcNow,
                Timezone = "UTC",
                Metadata = "{\"role\":\"moderator\"}",
                AccountId = 6,
                CreatedBy = "system",
                UpdatedBy = "system"
            }
        };
        }

        public static List<AccountType> GenerateAccountTypes()
        {
            return new List<AccountType>
            {
                new() { Id = 1, Name = "Free" }
            };

        }
    }
}
