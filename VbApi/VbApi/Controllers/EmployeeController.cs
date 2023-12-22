using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Employee 
{
    public string Name { get; set; }

  
    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }

  
    public string Phone { get; set; }

    public double HourlySalary { get; set; }

}

public class EmployeeValidator : AbstractValidator<Employee>
{
    public const string phone_regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
    private DateTime minAllowedBirthDate = DateTime.Today.AddYears(-65);
    private int minJuniorSalary=50,minSeniorSalary=200;
    public EmployeeValidator()
    {
        RuleFor(emp => emp.Name).NotEmpty().Must(name => name.Length >= 10 && name.Length <= 250)
    .WithMessage("Name must be between 10 and 250 characters.");
        RuleFor(emp => emp.DateOfBirth).NotEmpty().Must(Date => Date > minAllowedBirthDate).WithMessage("Birthdate is not valid.");
        RuleFor(emp => emp.Email).NotEmpty().WithMessage("E-mail can not be empty").EmailAddress().WithMessage("Invalid e-mail address");
        RuleFor(emp => emp.Phone).Must(IsPhoneNbr).WithMessage("Phone number is not valid");
        RuleFor(emp => emp.HourlySalary).Must(HourlySalary => HourlySalary >= 50 && HourlySalary <= 400).WithMessage("Hourly salary does not fall within allowed range(50,400).");
        RuleFor(emp => emp.HourlySalary)
             .Must((employee, hourlySalary) => IsHourlySalaryValid(hourlySalary, employee))
             .WithMessage("Minimum Senior hourly salary must be 200.");
    }

    public static bool IsPhoneNbr(string phone)
    {
        if (phone != null) return Regex.IsMatch(phone, phone_regex);
        return false;
    }
    public bool IsHourlySalaryValid(double hourlySalary, Employee employee)
    {
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirtyYears = employee.DateOfBirth <= dateBeforeThirtyYears;

        return isOlderThanThirtyYears ? hourlySalary >= minSeniorSalary : hourlySalary >= minJuniorSalary;
    }
}

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    public EmployeeController()
    {
    }

    [HttpPost]
    public IActionResult Post([FromBody] Employee value)
    {
        EmployeeValidator validator = new EmployeeValidator();
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