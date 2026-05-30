using material_design.DTO;
using material_design.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;

namespace material_design.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employees> _employeeRepo;
        private readonly IRepository<Post> _postRepo;

        public EmployeeService(IRepository<Employees> employeeRepo, IRepository<Post> postRepo)
        {
            _employeeRepo = employeeRepo;
            _postRepo = postRepo;
        }

        public List<EmployeeDto> GetAllEmployeesWithPost()
        {
            var employees = _employeeRepo.GetAll().ToList();
            var posts = _postRepo.GetAll().ToDictionary(p => p.id_post);

            return employees.Select(emp => new EmployeeDto
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

        public Employees GetEmployeeById(int id) => _employeeRepo.GetById(id);

        public void AddEmployee(Employees employee)
        {
            _employeeRepo.Add(employee);
            _employeeRepo.Save();
        }

        public void DeleteEmployee(int id)
        {
            var emp = _employeeRepo.GetById(id);
            if (emp != null)
            {
                _employeeRepo.Delete(emp);
                _employeeRepo.Save();
            }
        }

        public void UpdateEmployee(Employees employee)
        {
            _employeeRepo.Update(employee);
            _employeeRepo.Save();
        }

        public void ImportFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (string line in lines.Skip(1))
            {
                var values = line.Split(',');
                if (values.Length >= 5)
                {
                    var employee = new Employees
                    {
                        name_employee = values[0],
                        ph_number_emp = values[1],
                        post_emp_fk = int.Parse(values[2]),
                        email = values[3]
                    };
                    _employeeRepo.Add(employee);
                }
            }
            _employeeRepo.Save();
        }



    }
}