
namespace SuggestionAppLibrary.DataAccess
{
    public interface ISuggestionData
    {
        Task CreateSuggestion(SuggestionModel suggestion);
        Task<List<SuggestionModel>> GetAllApprovedSugeestions();
        Task<List<SuggestionModel>> GetAllSuggestions();
        Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval();
        Task<SuggestionModel> GetSuggestion(string id);
        Task<List<SuggestionModel>> GetUserSuggestion(string userId);
        Task<List<SuggestionModel>> GetUserSuggestions(string userId);
        Task UpdateSuggestion(SuggestionModel suggestion);
        Task UpVoteSuggestion(string suggestionId, string userId);
    }
}