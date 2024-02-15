using NovaLauncher.Views.Controls;
using System.Collections.Generic;

namespace NovaLauncher.Models.API.NovaBackend.Classes
{
    public class Account
    {
        public string account_id { get; set; }
        public string displayName { get; set; }
        public string access_token { get; set; }
        public bool accepted_tos { get; set; }
        public List<BuildInfo> builds { get; set; }
        public List<NovaStoreItem>? store { get; set; }

    }
}
