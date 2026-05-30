using System.Collections.Generic;
using System.Linq;
using material_design.Repositories;

namespace material_design.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly IRepository<Clients> _clientRepo;
        private readonly IRepository<Employees> _employeeRepo;

        public ReservationService(
            IRepository<Reservation> reservationRepo,
            IRepository<Clients> clientRepo,
            IRepository<Employees> employeeRepo)
        {
            _reservationRepo = reservationRepo;
            _clientRepo = clientRepo;
            _employeeRepo = employeeRepo;
        }

        public List<Reservation> GetAllReservations()
        {
            // Загружаем связанные данные
            var reservations = _reservationRepo.GetAll().ToList();
            var clients = _clientRepo.GetAll().ToDictionary(c => c.id_client);
            var employees = _employeeRepo.GetAll().ToDictionary(e => e.id_employee);
            foreach (var r in reservations)
            {
                if (clients.ContainsKey(r.id_client_fk))
                    r.Clients = clients[r.id_client_fk];
                if (employees.ContainsKey(r.id_employee_fk))
                    r.Employees = employees[r.id_employee_fk];
            }
            return reservations;
        }

        public List<Clients> GetClients() => _clientRepo.GetAll().ToList();
        public List<Employees> GetEmployees() => _employeeRepo.GetAll().ToList();

        public void AddReservation(Reservation reservation)
        {
            _reservationRepo.Add(reservation);
            _reservationRepo.Save();
        }

        public void UpdateReservation(Reservation reservation)
        {
            _reservationRepo.Update(reservation);
            _reservationRepo.Save();
        }

        public void DeleteReservation(int id)
        {
            var res = _reservationRepo.GetById(id);
            if (res != null)
            {
                _reservationRepo.Delete(res);
                _reservationRepo.Save();
            }
        }
    }
}