namespace BlogAPI.Interface;

public interface ITokenBlacklist
{
   void BlacklistToken(string token);
   bool isTokenBlacklisted(string token);
}