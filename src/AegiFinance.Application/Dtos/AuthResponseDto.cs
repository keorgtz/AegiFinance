namespace AegiFinance.Application.Dtos;

/// <summary>
/// Forma JSON-segura de la respuesta de autenticación: nunca incluye el refresh token,
/// que se entrega exclusivamente como cookie HttpOnly/Secure/SameSite=Lax.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new UserDto();
}
