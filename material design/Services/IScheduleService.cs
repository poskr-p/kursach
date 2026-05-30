using System;
using System.Collections.Generic;
using material_design.DTO;

namespace material_design.Services
{
    public interface IScheduleService
    {
        List<Employees> GetEmployees();
        void AddShift(int employeeId, DateTime date, TimeSpan start, TimeSpan end);
        List<EmployeeScheduleDto> GetScheduleForWeek(DateTime startDate);
    }
}