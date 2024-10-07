using System;
using System.ComponentModel.DataAnnotations;
using Bomberman.Models.Database;
using Bomberman.Services;

public class NoWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        
        if (value != null)
        {
            string? username = value.ToString();
            if (username == null || username.Contains(" "))
            {
                return new ValidationResult("Username shouldn't contain whitespace!");
            }
        }

        return ValidationResult.Success;
    }
}
public class UniqueUsernameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string? username = value.ToString();
            var userService = (IUserService?)validationContext.GetService(typeof(IUserService));

            // Check if the username already exists
            if (userService != null && (username == null || userService.GetUserByUsername(username) != null))
            {
                return new ValidationResult("Username is already taken!");
            }
        }

        return ValidationResult.Success;
    }
}
public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string? email = value.ToString();
            var userService = (IUserService?)validationContext.GetService(typeof(IUserService));

            // Check if the email already exists
            if (userService != null && (email == null || userService.EmailIsInUse(email)))
            {
                return new ValidationResult("Email is already in use!");
            }
        }

        return ValidationResult.Success;
    }
}
public class LoginSecurityCheckUsername : ValidationAttribute
{ //SHOULD CHECK WITH CHATGPT - NOW NEW ONE, ONLY WITH THE OLD VAN CUZ ITS ALREADY LEANRT
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string? username = value.ToString();
            var userservice = (IUserService?)validationContext.GetService(typeof(IUserService));

            /*
            if (userservice != null && userservice.ValidateLoginUsername(username) == false)
            {
                return new ValidationResult("Username or password is incorrect!");
            }*/
        }
        return ValidationResult.Success;
    }
}
public class LoginSecurityCheckPassword : ValidationAttribute
{ //SHOULD CHECK WITH CHETGPT
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string? password = value.ToString();
            var userservice = (IUserService?)validationContext.GetService(typeof(IUserService));

            /*
            if (userservice != null && userservice.ValidateLoginPassword(password) == false)
            {
                return new ValidationResult("Username or password is incorrect!");
            }*/
        }
        return ValidationResult.Success;
    }
}