using material_design;
using material_design.DTO;
using System.Collections.Generic;

public interface IEmployeeService
{
    List<EmployeeDto> GetAllEmployeesWithPost();
    Employees GetEmployeeById(int id);
    void AddEmployee(Employees employee);
    void DeleteEmployee(int id);
    void UpdateEmployee(Employees employee);
    void ImportFromCsv(string filePath);
     
}