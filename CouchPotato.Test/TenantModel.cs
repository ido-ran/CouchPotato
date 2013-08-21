using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Test {
  class TenantModel {
    public string TenantModelID { get; set; }

    public string Name { get; set; }

    //public virtual Plan Plan { get; set; }

    public string PlanID { get; set; }

    public string DefaultLanguage { get; set; }

    //public virtual ICollection<Survey> Surveys { get; set; }

    public virtual ICollection<UserModel> Users { get; set; }
  }
}
