namespace material_design.DTO
{
    public class EmployeeDto
    {
        public int id_employee { get; set; }
        public string name_employee { get; set; }
        public string ph_number_emp { get; set; }
        public int post_emp_fk { get; set; }
        public string email { get; set; }
        public string title_post { get; set; }
        public byte[] photo_data { get; set; }
    }
}