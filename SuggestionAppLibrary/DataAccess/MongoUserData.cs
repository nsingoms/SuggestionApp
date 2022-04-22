namespace SuggestionAppLibrary.DataAccess
{
    public class MongoUserData : IUserData
    {
        private readonly IMongoCollection<UserModel> _users;

        public MongoUserData(IDbConnection connection)
        {
            _users = connection.UserCollection;
        }
        public async Task<UserModel> GetUser(string id)
        {
            var result = await _users.FindAsync(u => u.Id == id);
            return result.FirstOrDefault();
        }
        public async Task<UserModel> GetUserFromAuthentication(string objectId)
        {
            var result = await _users.FindAsync(u => u.ObjectIdentifier == objectId);
            return result.FirstOrDefault();
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            var results = await _users.FindAsync(_ => true);
            return results.ToList();
        }
        public Task CreateUser(UserModel userModel)
        {
            return _users.InsertOneAsync(userModel);
        }
        public Task UpdateUser(UserModel userModel)
        {
            var filter = Builders<UserModel>.Filter.Eq("Id", userModel.Id);
            return _users.ReplaceOneAsync(filter, userModel, new ReplaceOptions { IsUpsert = true });
        }
    }
}
