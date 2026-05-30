using System;
using System.Collections.Generic;
using System.Linq;
using material_design.DTO;
using material_design.Repositories;

namespace material_design.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IRepository<Employees> _employeeRepo;
        private readonly IRepository<WorkSchedule> _scheduleRepo;
        private readonly IRepository<Post> _postRepo;

        public ScheduleService(
            IRepository<Employees> employeeRepo,
            IRepository<WorkSchedule> scheduleRepo,
            IRepository<Post> postRepo) // Добавили postRepo
        {
            _employeeRepo = employeeRepo;
            _scheduleRepo = scheduleRepo;
            _postRepo = postRepo;
        }

        public List<Employees> GetEmployees()
        {
            var employees = _employeeRepo.GetAll().ToList();
            var posts = _postRepo.GetAll().ToDictionary(p => p.id_post);

            // Загружаем Post для каждого сотрудника
            foreach (var emp in employees)
            {
                if (posts.ContainsKey(emp.post_emp_fk))
                    emp.Post = posts[emp.post_emp_fk];
            }

            return employees
                .Where(e => e.Post != null && (e.Post.title_post == "Официант" || e.Post.title_post == "Бармен"))
                .ToList();
        }

        public void AddShift(int employeeId, DateTime date, TimeSpan start, TimeSpan end)
        {
            var shift = new WorkSchedule
            {
                id_employee_fk = employeeId,
                work_date = date,
                start_time = start,
                end_time = end
            };
            _scheduleRepo.Add(shift);
            _scheduleRepo.Save();
        }

        public List<EmployeeScheduleDto> GetScheduleForWeek(DateTime startDate)
        {
            var employees = GetEmployees();
            var weekEnd = startDate.AddDays(7);
            var schedules = _scheduleRepo.GetAll()
                .Where(s => s.work_date >= startDate && s.work_date < weekEnd)
                .ToList();

            var result = new List<EmployeeScheduleDto>();

            foreach (var emp in employees)
            {
                var schedule = new EmployeeScheduleDto
                {
                    EmployeeName = emp.name_employee
                };

                for (int i = 0; i < 7; i++)
                {
                    var day = startDate.AddDays(i);
                    var daySchedules = schedules
                        .Where(s => s.id_employee_fk == emp.id_employee && s.work_date == day)
                        .ToList();

                    string shiftText = daySchedules.Any() ?
                        string.Join(", ", daySchedules.Select(s => $"{s.start_time:hh\\:mm}-{s.end_time:hh\\:mm}")) :
                        "Выходной";

                    switch (i)
                    {
                        case 0: schedule.Monday = shiftText; break;
                        case 1: schedule.Tuesday = shiftText; break;
                        case 2: schedule.Wednesday = shiftText; break;
                        case 3: schedule.Thursday = shiftText; break;
                        case 4: schedule.Friday = shiftText; break;
                        case 5: schedule.Saturday = shiftText; break;
                        case 6: schedule.Sunday = shiftText; break;
                    }
                }

                result.Add(schedule);
            }

            return result;
        }
    }
}