namespace SuggestionAppUI.Helpers
{
    public static class AuthenticationStateProviverHelpers
    {
        public static async Task<UserModel> GetUserFromAuth(this AuthenticationStateProvider provider, IUserData userData)
        {
            var authState = await provider.GetAuthenticationStateAsync();
            string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
            return await userData.GetUserFromAuthentication(objectId);
        }
    }
}
