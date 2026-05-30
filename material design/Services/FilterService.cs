using System.Collections.Generic;
using System.Linq;
using material_design.DTO;
using material_design.Repositories;

namespace material_design.Services
{
    public class FilterService : IFilterService
    {
        private readonly IRepository<Employees> _employeeRepo;
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Clients> _clientRepo;

        public FilterService(
            IRepository<Employees> employeeRepo,
            IRepository<Post> postRepo,
            IRepository<Clients> clientRepo)
        {
            _employeeRepo = employeeRepo;
            _postRepo = postRepo;
            _clientRepo = clientRepo;
        }

        public List<EmployeeDto> GetEmployees(string search = null)
        {
            var employees = _employeeRepo.GetAll().AsQueryable();
            if (!string.IsNullOrEmpty(search))
                employees = employees.Where(e => e.name_employee.Contains(search));

            var posts = _postRepo.GetAll().ToDictionary(p => p.id_post);

            return employees.ToList().Select(emp => new EmployeeDto
            {
                id_employee = emp.id_employee,
                name_employee = emp.name_employee,
                ph_number_emp = emp.ph_number_emp,
                post_emp_fk = emp.post_emp_fk,
                email = emp.email,
                photo_data = emp.photo_data,
                title_post = posts.ContainsKey(emp.post_emp_fk) ? posts[emp.post_emp_fk].title_post : null
            }).ToList();
        }

        public List<ClientDto> GetClients(string search = null)
        {
            var clients = _clientRepo.GetAll().AsQueryable();
            if (!string.IsNullOrEmpty(search))
                clients = clients.Where(c => c.name_client.Contains(search));

            return clients.Select(c => new ClientDto
            {
                id_client = c.id_client,
                name_client = c.name_client,
                ph_numb_client = c.ph_numb_client
            }).ToList();
        }
    }
}