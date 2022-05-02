using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess
{
    public class MongoSuggestionData : ISuggestionData
    {
        private readonly IDbConnection _db;
        private readonly IUserData _userData;
        private readonly IMemoryCache _cache;
        private readonly IMongoCollection<SuggestionModel> _suggestions;
        private const string CacheName = "suggestionData";

        public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
        {
            _db = db;
            _userData = userData;
            _cache = cache;
            _suggestions = db.SuggestionCollection;
        }

        public async Task<List<SuggestionModel>> GetAllSuggestions()
        {
            var output = _cache.Get<List<SuggestionModel>>(CacheName);
            if (output is null)
            {
                var results = await _suggestions.FindAsync(s => s.Archived == false);
                output = results.ToList();

                _cache.Set(CacheName, output, TimeSpan.FromMinutes(10));
            }

            return output;
        }
        public async Task<List<SuggestionModel>> GetUserSuggestions(string userId)
        {
            var output = _cache.Get<List<SuggestionModel>>(userId);
            if (output is null)
            {
                var results = await _suggestions.FindAsync(s => s.Author.Id == userId );
                output = results.ToList();

                _cache.Set(userId, output, TimeSpan.FromMinutes(1));
            }

            return output;
        }
        public async Task<List<SuggestionModel>> GetAllApprovedSugeestions()
        {
            var output = await GetAllSuggestions();
            return output.Where(x => x.ApprovedForRelease).ToList();

        }
        public async Task<SuggestionModel> GetSuggestion(string id)
        {
            var results = await _suggestions.FindAsync(s => s.Id == id);

            return results.FirstOrDefault();
        }
        public async Task<List<SuggestionModel>> GetUserSuggestion(string userId)
        {
            var output = _cache.Get<List<SuggestionModel>>(userId);

            if (output is null)
            {
                var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
                output = results.ToList();

                _cache.Set(userId, userId, TimeSpan.FromMinutes(1));
            }
            return output;
        }
        public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
        {
            var output = await GetAllSuggestions();

            return output.Where(x => x.ApprovedForRelease == false && x.Rejected == false).ToList();
        }
        public async Task UpdateSuggestion(SuggestionModel suggestion)
        {
            await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
            _cache.Remove(CacheName);
        }

        public async Task UpVoteSuggestion(string suggestionId, string userId)
        {
            var client = _db.Client;
            using var session = await client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
                var suggestion = (await suggestionsInTransaction.FindAsync(session, s => s.Id == suggestionId)).First();

                bool isUpVote = suggestion.UserVotes.Add(userId);
                
                if (isUpVote == false)
                {
                    suggestion.UserVotes.Remove(userId);
                }

                await suggestionsInTransaction.ReplaceOneAsync(session, s => s.Id == suggestionId, suggestion);

                var usersinTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUser(userId);

                if (isUpVote ==true)
                {
                    user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
                }
                else
                {
                    var suggestionToremove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                    user.VotedOnSuggestions.Remove(suggestionToremove);
                }
                await usersinTransaction.ReplaceOneAsync(s => s.Id == userId, user);
                await session.CommitTransactionAsync();
                _cache.Remove(CacheName);

            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }

        }

        public async Task CreateSuggestion(SuggestionModel suggestion)
        {
            var client = _db.Client;
            using var session = await client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
                await suggestionsInTransaction.InsertOneAsync(session,suggestion);

                var usersinTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUser(suggestion.Author.Id);
                user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));

                await usersinTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);
                await session.CommitTransactionAsync();


            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }

        }
    }
}
