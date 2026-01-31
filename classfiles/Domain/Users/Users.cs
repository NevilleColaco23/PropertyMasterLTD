using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;
using MyWarehouse.Domain.Partners;
using MyWarehouse.Domain.Products;
using System.Diagnostics.CodeAnalysis;

namespace MyWarehouse.Domain.Users
{
    public class Users : IEntity<int>
    {
        public int Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; protected set; }
        public string NormalizedUserName { get; protected set; }

        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public int Version { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EmailConfirmationTokenHash { get; set; }
        public DateTime EmailConfirmationTokenExpiresAtUtc { get; set; }
        public DateTime EmailConfirmationTokenCreatedAtUtc { get; set; }
        public List<PropertyAccessList> PropertyAccessList { get; set; }

        private Users()
        {
            
        }

        public Users(string username, string email, string password, string phoneNumber)
        {
            UpdateName(username);
            UpdateEmail(email);
            UpdatePassword(password);
            UpdatePhone(phoneNumber);
        }

        [MemberNotNull(nameof(UserName))]
        public void UpdateName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("User Name cannot be empty.");

            UserName = value;
        }

        [MemberNotNull(nameof(Email))]
        public void UpdateEmail(string value) //TODO: to check if email exists and email format
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");

            if (value.Length > PartnerInvariants.NameMaxLength)
                throw new ArgumentException($"Length of value ({value.Length}) exceeds maximum name length ({ProductInvariants.NameMaxLength}).");

            Email = value;
        }

        [MemberNotNull(nameof(PasswordHash))]
        public void UpdatePassword(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Password cannot be empty.");

            if (value.Length > PartnerInvariants.NameMaxLength)
                throw new ArgumentException($"Length of value ({value.Length}) exceeds maximum name length ({ProductInvariants.NameMaxLength}).");

            PasswordHash = value;
        }

        [MemberNotNull(nameof(PhoneNumber))]
        public void UpdatePhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone Number cannot be empty.");

            PhoneNumber = value;
        }
    }
    public class PropertyAccessList
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
