using System.Collections.Generic;
using material_design.DTO;

namespace material_design.Services
{
    public interface IFilterService
    {
        List<EmployeeDto> GetEmployees(string search = null);
        List<ClientDto> GetClients(string search = null);
    }
}