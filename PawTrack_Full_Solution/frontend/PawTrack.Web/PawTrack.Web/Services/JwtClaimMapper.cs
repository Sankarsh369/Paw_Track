using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PawTrack.Web.Services
{
    // JwtSecurityTokenHandler writes ClaimTypes.NameIdentifier/Name/Role using the
    // short JWT claim names ("nameid", "unique_name", "role") per its default outbound
    // claim map. When we read the token back with ReadJwtToken (not ValidateToken),
    // those short names come back as-is — so we translate them back to the full
    // ClaimTypes URIs the rest of the app (and [Authorize(Roles=...)]) expects.
    public static class JwtClaimMapper
    {
        public static ClaimsIdentity ToCookieIdentity(string jwtToken, string authenticationType)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtToken);

            var nameId = jwt.Claims.FirstOrDefault(c => c.Type is "nameid" or "sub" or ClaimTypes.NameIdentifier)?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type is "unique_name" or "name" or ClaimTypes.Name)?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type is "role" or ClaimTypes.Role)?.Value;

            var claims = new List<Claim>();
            if (nameId is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId));
            if (name is not null) claims.Add(new Claim(ClaimTypes.Name, name));
            if (role is not null) claims.Add(new Claim(ClaimTypes.Role, role));

            return new ClaimsIdentity(claims, authenticationType);
        }
    }
}
