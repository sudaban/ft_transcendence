namespace Backend.Application.DTOs.Responses.Users;

public record UserDto(
    string Id,          // Veritabanında int, frontend string bekliyor (.ToString() ile dönüştüreceğiz)
    string Username,
    string Handle,      // Frontend @username formatında bekliyor
    string Avatar       // Veritabanındaki ProfilePictureUrl ile eşleşecek
);