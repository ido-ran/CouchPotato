using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  internal class PreProcessInfo {
    private readonly PreProcessEntityInfo[] rows;

    public PreProcessInfo(PreProcessEntityInfo[] rows) {
      this.rows = rows;
    }

    public PreProcessEntityInfo[] Rows { get { return rows; } }
  }
}
