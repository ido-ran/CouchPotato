using CouchPotato.LoveSeatAdapter;
using CouchPotato.Odm;
using LoveSeat;

namespace CouchPotato.CouchClientAdapter {
  public class CouchPotatoClientAdapterFactory {

    public CouchDBClientAdapter LoveSeat(
      string databaseName, 
      string host, 
      int port, 
      string username, 
      string password, 
      bool ssl) {

      // TODO: Load configuration from app.config
      CouchClient client = new CouchClient(host, port, username, password, ssl, AuthenticationType.Basic);
      CouchDatabase db = client.GetDatabase(databaseName);
      return new LoveSeatClientAdapter(db);
    }
  }
}
