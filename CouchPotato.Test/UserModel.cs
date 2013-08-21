using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Annotations;

namespace CouchPotato.Test {
  class UserModel {
    public string UserModelID { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordSalt { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string[] Roles { get; set; }
    public string[] Tags { get; set; }

    [Association("Users")]
    public virtual ICollection<TenantModel> Tenants { get; set; }
  }
}
