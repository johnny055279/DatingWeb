namespace Dating_WebAPI.Entities
{
    public class Connection
    {

        // for entity framework constructor, when create a table, it need it
        public Connection(){}


        public Connection(string connectionId, string userName){
            ConnectionId = connectionId;
            UserName = userName;
        }

        public string ConnectionId { get; set; }

        public string UserName { get; set; }
    }
}