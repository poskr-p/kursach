using System;
using System.Collections.Generic;
using material_design.DTO;

namespace material_design.Services
{
    public interface IReportService
    {
        List<SalesReportDto> GetSalesReport(DateTime start, DateTime end);
        List<EmployeeReportDto> GetEmployeeReport(DateTime start, DateTime end);
        List<ClientReportDto> GetClientReport();
        List<MenuReportDto> GetMenuReport(DateTime start, DateTime end);
        List<FinancialReportDto> GetFinancialReport(DateTime start, DateTime end);
    }
}