using System.Collections.Generic;

namespace material_design.Services
{
    public interface IReservationService
    {
        List<Reservation> GetAllReservations();
        List<Clients> GetClients();
        List<Employees> GetEmployees();
        void AddReservation(Reservation reservation);
        void UpdateReservation(Reservation reservation);
        void DeleteReservation(int id);
    }
}