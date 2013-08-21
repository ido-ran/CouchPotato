using CouchPotato.LoveSeatAdapter;
using CouchPotato.Odm;
using LoveSeat;

namespace CouchPotato.CouchClientAdapter {
  public class CouchPotatoClientAdapterFactory {

    public CouchDBClientAdapter LoveSeat(string databaseName) {
      // TODO: Load configuration from app.config
      CouchClient client = new CouchClient();
      CouchDatabase db = client.GetDatabase(databaseName);
      return new LoveSeatClientAdapter(db);
    }
  }
}
