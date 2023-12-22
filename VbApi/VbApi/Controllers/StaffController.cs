using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;
using System.Text.RegularExpressions;

namespace VbApi.Controllers;

public class Staff
{
    [Required]
    [MinLength(1)]
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public decimal? HourlySalary { get; set; }
}


public class StaffValidator : AbstractValidator<Staff>
{
    public const string phone_regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
    public StaffValidator()
    {
        RuleFor(staff => staff.Name).NotEmpty().Must(name => name.Length >= 10 && name.Length <= 250)
    .WithMessage("Name must be between 10 and 250 characters.");
        RuleFor(staff => staff.Email).NotEmpty().WithMessage("E-mail can not be empty").EmailAddress().WithMessage("Invalid e-mail address");
        RuleFor(staff => staff.Phone).NotEmpty().WithMessage("Phone number can not be emmpty");
        RuleFor(staff => staff.Phone).Must(IsPhoneNbr).WithMessage("Phone number is not valid");
        RuleFor(staff => staff.HourlySalary).NotEmpty().Must(Is_Hourly_Salary_Valid).WithMessage("Hourly salary does not fall within allowed range(30,400).");
    }

    private bool Is_Hourly_Salary_Valid(decimal? salary)
    {
        if (salary >= 30 && salary <= 400) return true;
        return false;
    }

    public static bool IsPhoneNbr(string phone)
    {
        if (phone != null) return Regex.IsMatch(phone, phone_regex);
        return false;
    }
}


[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{

    public StaffController()
    {
    }

    [HttpPost]
    public IActionResult Post([FromBody] Staff value)
    {
        StaffValidator validator = new StaffValidator();
        FluentValidation.Results.ValidationResult is_valid = validator.Validate(value);
        if (!is_valid.IsValid)
        {
            foreach (var item in is_valid.Errors)
            {
                ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
            }
            return BadRequest(ModelState);
        }
        return Ok(value);
    }
}