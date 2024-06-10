using BlogAPI.Interface;

namespace BlogAPI.Repository;

public class TokenBlacklist : ITokenBlacklist
{
    public readonly HashSet<string> _blacklistedToken = new HashSet<string>();
    
    public void BlacklistToken(string token)
    {
        _blacklistedToken.Add(token);
    }

    public bool isTokenBlacklisted(string token)
    {
        return _blacklistedToken.Contains(token);
    }
}