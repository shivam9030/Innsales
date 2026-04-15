
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace InnSales.Domain.UserManagement.Entities
{

    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public DateTime JoinedDate { get; set; }
        public string Department { get; set; }
        public string OfficeLocation { get; set; }


        public string? WebhookUrl { get; set; }
        public string? WebhookSecret { get; set; }
        public string? SubscriptionName { get; set; }

    }
}
