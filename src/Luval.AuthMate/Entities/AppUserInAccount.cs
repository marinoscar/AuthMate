﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luval.AuthMate.Entities
{

    /// <summary>
    /// Represents the relationship between a user and an account in the system.
    /// </summary>
    [Table("AppUserInAccount")]
    public class AppUserInAccount : BaseEntity
    {
        /// <summary>
        /// The unique identifier for the UserInAccount relationship.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key referencing the User table.
        /// </summary>
        [Required]
        public ulong AppUserId { get; set; }

        /// <summary>
        /// Navigation property for the User.
        /// </summary>
        [ForeignKey(nameof(AppUserId))]
        public AppUser User { get; set; }

        /// <summary>
        /// The foreign key referencing the Account table.
        /// </summary>
        [Required]
        public ulong AccountId { get; set; }

        /// <summary>
        /// Navigation property for the Account.
        /// </summary>
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        #region Control Fields

        /// <summary>
        /// The UTC timestamp when the record was created.
        /// </summary>
        public DateTime UtcCreatedOn { get; set; }

        /// <summary>
        /// The user who created the record.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC timestamp when the record was last updated.
        /// </summary>
        public DateTime UtcUpdatedOn { get; set; }

        /// <summary>
        /// The user who last updated the record.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// The version of the record, incremented on updates.
        /// </summary>
        [ConcurrencyCheck]
        public uint Version { get; set; }

        #endregion

        /// <summary>
        /// Initializes the control fields for the entity.
        /// </summary>
        public AppUserInAccount()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }
    }

}